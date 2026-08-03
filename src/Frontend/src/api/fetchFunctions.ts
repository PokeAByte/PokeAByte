export const BASE_URL = "http://localhost:8085";
export const DEFAULT_HEADERS = { "Content-Type": "application/json" };

export type Serializable = Record<string, string|number> | string[] | string | number | null;

export async function fetchWithBody<T extends Serializable>(method: "PUT" | "POST", requestUrl: string, data: T) {
	try {
		const response = await fetch(
			BASE_URL + requestUrl,
			{
				method: method,
				body: JSON.stringify(data),
				headers: DEFAULT_HEADERS,
			}
		);
		return response.ok || await response.json() as string;
	} catch {
		return false;
	}
}

export async function fetchGet<T>(requestUrl: string): Promise<T|null> {
	try {
		const response = await fetch(
			BASE_URL + requestUrl,
			{
				method: "GET",
				headers: DEFAULT_HEADERS,
			}
		);
		return response.ok 
			? await response.json() as T
			: null;
	} catch {
		return null;
	}
}

export async function postWithoutResult<T>(requestUrl: string, body: null | T = null) {
	try {
		const response = await fetch(
			BASE_URL + requestUrl,
			{
				method: "POST",
				body: JSON.stringify(body),
				headers: DEFAULT_HEADERS,
			}
		);
		return response.ok;
	} catch {
		return false
	}
}
