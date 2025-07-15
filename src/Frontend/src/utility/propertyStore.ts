import { ChangedField, GameProperty, Glossary, PokeAClient, ProblemDetails } from "pokeaclient";
import { Toasts } from "../notifications/ToastStore";

type Callback = () => void;

export class PropertyStore {
	private _connectionSubscriber: ((connected: boolean) => void)[] = [];
	private _mapperSubscriber: Callback[] = [];
	private _propertyCallbacks: Record<string, Callback[]> = {};
	client: PokeAClient;
	private _pendingPathUpdates: string[];

	/**
	 * Creates an instance of PropertyStore.
	 */
	constructor() {
		this._pendingPathUpdates = [];
		this.client = new PokeAClient({
			onMapperChange: this.onMapperChange,
			onPropertiesChanged: this.onPropertiesChange,
			onConnectionChange: this.onConnectionChange,
			onError: this.onError,
		}, {
			updateOn: [ChangedField.bytes, ChangedField.value, ChangedField.frozen]
		});
		this.client.connect();
		window.setInterval(() => this.sendPropertyChanges(), 32);
	}

	/**
	 * Send property changes to subscribers.
	 */
	sendPropertyChanges = () => {
		if (this._pendingPathUpdates.length > 0) {
			this._pendingPathUpdates.forEach((path) => {
				this._propertyCallbacks[path]?.forEach(x => x());
			});
			this._pendingPathUpdates.length = 0;
		}
	}

	onPropertiesChange = (paths: string[]) => {
		paths.forEach(path => {
			if (!this._pendingPathUpdates.includes(path)) {
				this._pendingPathUpdates.push(path);
			}
		})
	}

	onMapperChange = () => {
		this._mapperSubscriber.forEach(callback => callback());
	}

	onConnectionChange = (connected: boolean) => {
		this._connectionSubscriber.forEach(callback => callback(connected));
	}

	onError = (error: ProblemDetails) => {
		Toasts.push(`${error.title}: ${error.detail}`, "", "error", false);
	}

	subscribeProperty = (path: string) => (onStoreChange: () => void) => {
		if (!this._propertyCallbacks[path]) {
			this._propertyCallbacks[path] = [];
		}
		this._propertyCallbacks[path].push(onStoreChange)
		return () => {
			this._propertyCallbacks[path] = this._propertyCallbacks[path].filter(x => x != onStoreChange);
		}
	}

	subscribeMapper = (onStoreChange: () => void) => {
		this._mapperSubscriber.push(onStoreChange)
		return () => {
			this._mapperSubscriber = this._mapperSubscriber.filter(x => x != onStoreChange);
		}
	}

	subscribeConnected = (onConnectedChange: () => void) => {
		this._connectionSubscriber.push(onConnectedChange);
		return () => {
			this._connectionSubscriber = this._connectionSubscriber.filter(x => x != onConnectedChange);
		}
	}
	isConnected = () => this.client.isConnected();

	getMapper = () => this.client.getMapper();


	getProperty = <T = any>(path: string) => this.client.getProperty<T>(path);
	getAllProperties = (): Record<string, GameProperty> => this.client["_properties"];
	getGlossary = (): Glossary|null => this.client.getGlossary();

	getGlossaryItem = (item: string) => {
		const glossary = this.client.getGlossary();
		if (!glossary) {
			return null;
		}
		return glossary[item];
	}
}

export const Store = new PropertyStore(); 