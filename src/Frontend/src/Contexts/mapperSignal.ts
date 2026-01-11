import { Store } from "@/utility/propertyStore";
import { signal } from "@preact/signals";
import { Mapper } from "pokeaclient";

export const mapperSignal = signal<Mapper|null>(Store.getMapper());
export const isConnectedSignal = signal<true|false>(Store.isConnected());

Store.subscribeMapper(() => {
	mapperSignal.value = Store.getMapper();
});

Store.subscribeConnected(() => {
	isConnectedSignal.value = Store.isConnected();
});