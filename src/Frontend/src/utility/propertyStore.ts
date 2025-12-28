import { ChangedField, GameProperty, Glossary, PokeAClient, ProblemDetails } from "pokeaclient";
import { Toasts } from "../notifications/ToastStore";

type Callback = () => void;
type UpdateCallback = (path: string) => void;

export class PropertyStore {
	private _updateListener: UpdateCallback[] = [];
	private _connectionSubscriber: ((connected: boolean) => void)[] = [];
	private _mapperSubscriber: Callback[] = [];
	client: PokeAClient;
	private _pendingPathUpdates: string[];
	private _suppressUpdate: boolean = false;

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
		window.setInterval(() => {
			this.sendPropertyChanges()
			
		}, 32);
	}

	reloadMapper = async () => {
		const mapperFileId = this.client.getMapper()?.fileId;
		if (!mapperFileId) {
			return;
		}
		this._suppressUpdate = true;
		await Store.client.unloadMapper();
		Toasts.clearErrors();
		const success = await Store.client.changeMapper(mapperFileId);
		this._suppressUpdate = false;
		this.onMapperChange();
		return success;
	}

	/**
	 * Send property changes to subscribers.
	 */
	sendPropertyChanges = () => {
		this._pendingPathUpdates.forEach((path) => {
			this._updateListener.forEach(callback => callback(path));
		});
		this._pendingPathUpdates.length = 0;
	}

	onPropertiesChange = (paths: string[]) => {
		paths.forEach(path => {
			if (!this._pendingPathUpdates.includes(path)) {
				this._pendingPathUpdates.push(path);
			}
		})
	}

	onMapperChange = () => {
		if (!this._suppressUpdate) {
			this._mapperSubscriber.forEach(callback => callback());
		}
	}

	onConnectionChange = (connected: boolean) => {
		this._connectionSubscriber.forEach(callback => callback(connected));
	}

	onError = (error: ProblemDetails) => {
		Toasts.push(`${error.title}: ${error.detail}`, "", "red", false);
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

	addUpdateListener = (callback: UpdateCallback) => {
		this._updateListener.push(callback);
	}

	removeUpdateListener = (callback: UpdateCallback) => {
		this._updateListener = this._updateListener.filter(x => x !== callback);
	}
}

export const Store = new PropertyStore(); 