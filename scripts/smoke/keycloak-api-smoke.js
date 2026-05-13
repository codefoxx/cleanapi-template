import encoding from "k6/encoding";
import http from "k6/http";
import {check, fail} from "k6";

export const options = {
    vus: 1,
    iterations: 1,
    thresholds: {
        checks: ["rate==1.0"],
        http_req_failed: ["rate<0.01"],
    },
};

http.setResponseCallback(
    http.expectedStatuses(
        {min: 200, max: 399},
        400,
        401,
        404
    )
);

const keycloakUrl = __ENV.KC_URL || "http://localhost:8080";
const apiUrl = __ENV.API_URL || "http://localhost:5080";
const realm = __ENV.KC_REALM || "acme-products";
const clientId = __ENV.KC_CLIENT_ID || "acme-products-api";
const clientSecret = __ENV.KC_CLIENT_SECRET || "local-dev-secret";

const unknownProductId = "11111111-1111-1111-1111-111111111111";

export default function () {
    verifyRootEndpoint();
    verifyProtectedEndpointsRequireAuthentication();

    const token = getAccessToken();

    verifyTokenPayload(token);

    verifyInvalidCreateRequestReturnsBadRequest(token);

    const product = createProduct(token);

    getProduct(token, product.id, product);
    verifyUnknownProductReturnsNotFound(token);

    const changedProduct = changeProductPrice(token, product.id);
    getProduct(token, product.id, changedProduct);

    verifyInvalidPriceChangeReturnsBadRequest(token, product.id);

    discontinueProduct(token, product.id);
    verifyProductIsDiscontinued(token, product.id);
}

function verifyRootEndpoint() {
    const response = http.get(`${apiUrl}/`);

    check(response, {
        "GET / returns 200": r => r.status === 200,
    });
}

function verifyProtectedEndpointsRequireAuthentication() {
    const readResponse = http.get(`${apiUrl}/api/products/${unknownProductId}`);

    check(readResponse, {
        "GET product without token returns 401": r => r.status === 401,
    });

    const createResponse = http.post(
        `${apiUrl}/api/products`,
        JSON.stringify({
            name: "Unauthorized Keyboard",
            price: 99.99,
            currency: "USD",
        }),
        jsonHeaders()
    );

    check(createResponse, {
        "POST product without token returns 401": r => r.status === 401,
    });

    const changePriceResponse = http.put(
        `${apiUrl}/api/products/${unknownProductId}/price`,
        JSON.stringify({
            price: 129.99,
            currency: "USD",
        }),
        jsonHeaders()
    );

    check(changePriceResponse, {
        "PUT product price without token returns 401": r => r.status === 401,
    });

    const discontinueResponse = http.post(
        `${apiUrl}/api/products/${unknownProductId}/discontinue`,
        null,
        jsonHeaders()
    );

    check(discontinueResponse, {
        "POST discontinue without token returns 401": r => r.status === 401,
    });
}

function getAccessToken() {
    const response = http.post(
        `${keycloakUrl}/realms/${realm}/protocol/openid-connect/token`,
        {
            grant_type: "client_credentials",
            client_id: clientId,
            client_secret: clientSecret,
        },
        {
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
            },
        }
    );

    check(response, {
        "token request returns 200": r => r.status === 200,
        "token response contains access_token": r => Boolean(r.json("access_token")),
    });

    if (response.status !== 200) {
        fail(`Token request failed: ${response.status} ${response.body}`);
    }

    return response.json("access_token");
}

function verifyTokenPayload(token) {
    const payload = decodeJwtPayload(token);

    check(payload, {
        "token issuer matches configured realm": p =>
            p.iss === `${keycloakUrl}/realms/${realm}`,

        "token audience contains API audience": p =>
            Array.isArray(p.aud)
                ? p.aud.includes(clientId)
                : p.aud === clientId,

        "token contains products.read scope": p =>
            typeof p.scope === "string" && p.scope.split(" ").includes("products.read"),

        "token contains products.write scope": p =>
            typeof p.scope === "string" && p.scope.split(" ").includes("products.write"),
    });
}

function verifyInvalidCreateRequestReturnsBadRequest(token) {
    const response = http.post(
        `${apiUrl}/api/products`,
        JSON.stringify({
            name: "",
            price: 10,
            currency: "USD",
        }),
        authJsonHeaders(token)
    );

    check(response, {
        "POST invalid product returns 400": r => r.status === 400,
    });
}

function createProduct(token) {
    const expected = {
        name: `Keyboard ${Date.now()}`,
        price: 99.99,
        currency: "USD",
    };

    const response = http.post(
        `${apiUrl}/api/products`,
        JSON.stringify(expected),
        authJsonHeaders(token)
    );

    check(response, {
        "POST valid product returns 201": r => r.status === 201,
        "POST valid product returns id": r => Boolean(r.json("id")),
        "POST valid product returns name": r => r.json("name") === expected.name,
        "POST valid product returns price": r => Number(r.json("price")) === expected.price,
        "POST valid product returns currency": r => r.json("currency") === expected.currency,
        "POST valid product returns active status": r => r.json("status") === "Active",
    });

    if (response.status !== 201) {
        fail(`Create product failed: ${response.status} ${response.body}`);
    }

    return {
        id: response.json("id"),
        name: expected.name,
        price: expected.price,
        currency: expected.currency,
        status: "Active",
    };
}

function getProduct(token, productId, expected) {
    const response = http.get(
        `${apiUrl}/api/products/${productId}`,
        authHeaders(token)
    );

    check(response, {
        "GET existing product returns 200": r => r.status === 200,
        "GET existing product returns same id": r => r.json("id") === productId,
        "GET existing product returns expected name": r => r.json("name") === expected.name,
        "GET existing product returns expected price": r => Number(r.json("price")) === expected.price,
        "GET existing product returns expected currency": r => r.json("currency") === expected.currency,
        "GET existing product returns expected status": r => r.json("status") === expected.status,
    });

    if (response.status !== 200) {
        fail(`Get product failed: ${response.status} ${response.body}`);
    }
}

function verifyUnknownProductReturnsNotFound(token) {
    const response = http.get(
        `${apiUrl}/api/products/${unknownProductId}`,
        authHeaders(token)
    );

    check(response, {
        "GET unknown product with token returns 404": r => r.status === 404,
    });
}

function changeProductPrice(token, productId) {
    const expected = {
        price: 129.99,
        currency: "USD",
    };

    const response = http.put(
        `${apiUrl}/api/products/${productId}/price`,
        JSON.stringify(expected),
        authJsonHeaders(token)
    );

    check(response, {
        "PUT product price returns 200": r => r.status === 200,
        "PUT product price returns same id": r => r.json("id") === productId,
        "PUT product price returns changed price": r => Number(r.json("price")) === expected.price,
        "PUT product price returns changed currency": r => r.json("currency") === expected.currency,
    });

    if (response.status !== 200) {
        fail(`Change product price failed: ${response.status} ${response.body}`);
    }

    return {
        id: productId,
        name: response.json("name"),
        price: expected.price,
        currency: expected.currency,
        status: response.json("status"),
    };
}

function verifyInvalidPriceChangeReturnsBadRequest(token, productId) {
    const response = http.put(
        `${apiUrl}/api/products/${productId}/price`,
        JSON.stringify({
            price: -1,
            currency: "USD",
        }),
        authJsonHeaders(token)
    );

    check(response, {
        "PUT invalid product price returns 400": r => r.status === 400,
    });
}

function discontinueProduct(token, productId) {
    const response = http.post(
        `${apiUrl}/api/products/${productId}/discontinue`,
        null,
        authHeaders(token)
    );

    check(response, {
        "POST discontinue product returns success": r =>
            r.status === 200 || r.status === 204,
    });

    if (response.status !== 200 && response.status !== 204) {
        fail(`Discontinue product failed: ${response.status} ${response.body}`);
    }
}

function verifyProductIsDiscontinued(token, productId) {
    const response = http.get(
        `${apiUrl}/api/products/${productId}`,
        authHeaders(token)
    );

    check(response, {
        "GET discontinued product returns 200": r => r.status === 200,
        "GET discontinued product returns Discontinued status": r =>
            r.json("status") === "Discontinued",
    });
}

function authHeaders(token, additionalHeaders = {}) {
    return {
        headers: {
            Authorization: `Bearer ${token}`,
            ...additionalHeaders,
        },
    };
}

function jsonHeaders(additionalHeaders = {}) {
    return {
        headers: {
            "Content-Type": "application/json",
            ...additionalHeaders,
        },
    };
}

function authJsonHeaders(token) {
    return authHeaders(token, {
        "Content-Type": "application/json",
    });
}

function decodeJwtPayload(token) {
    const parts = token.split(".");

    if (parts.length !== 3) {
        fail("Access token is not a JWT.");
    }

    const payload = parts[1]
        .replace(/-/g, "+")
        .replace(/_/g, "/");

    const paddedPayload = payload.padEnd(
        payload.length + ((4 - payload.length % 4) % 4),
        "="
    );

    return JSON.parse(encoding.b64decode(paddedPayload, "std", "s"));
}