import { GameProperty, GamePropertyType } from "pokeaclient";
import { useState } from "preact/hooks";
import { Store } from "@/utility/propertyStore";

export function getPropertyFieldValue(property: GameProperty<any>|null) {
	if (!property) {
		return null;
	}
	if (property.reference && property.type !== "string") {
		const glossaryItems = Store.getGlossaryItem(property!.reference!) ?? [];
		return glossaryItems.find(x => x.value?.toString() === property!.value?.toString())?.key
	}
	if (property.type === "bitArray" && property.value) {
		return property.value.map((x: boolean) => x ? "1" : "0").join("") ?? "";
	}
	if (property.type === "byteArray" && property.value) {
		return property.value.map((x:number) => x.toString(16).toUpperCase().padStart(2, "0")).join(" ");

	}
	return property.value;
}

export type PropertyTextboxProps = {
	path: string,
	type: string,
	isEdit: boolean,
	isReadonly: boolean,
	propertyType: GamePropertyType | null,
	editValue: string | boolean | number | number[] | null,
	actualValue: any,
	placeholder?: string,
	save: () => void,
	setValue: (value: string|boolean) => void,
}

export function PropertyTextbox(props: PropertyTextboxProps) {
	const value = props.actualValue;
	const [focusValue, setFocusValue] = useState<string|boolean|null>(null);
	const [hasFocus, setHasFocus] = useState<boolean>(false);
	const update = (value: string | boolean) => {
		if (!props.isReadonly) {
			props.setValue(value);
		}
	};
	if (props.type === "checkbox") {
		const checked = props.isEdit ? props.editValue : value;
		return (
			<label >
				<input
					title={checked?.toString() ?? "false"}
					type="checkbox"
					role="switch"
					checked={checked}
					onKeyDown={(e) => {
						if (e.key === "Enter") {
							props.save();
						}
					}}
					onInput={(e) => update(e.currentTarget.checked)}
					readOnly={props.isReadonly}
					disabled={props.isReadonly} 
				/>
			</label>
		);
	}
	const actualValue = hasFocus ? focusValue : value;
	return (
		<input
			placeholder={props.placeholder || (!props.isEdit && actualValue === null ? "null" : "")}
			type={props.type}
			value={props.isEdit ? props.editValue : actualValue}
			onFocus={() => { setHasFocus(true); setFocusValue(value) }}
			onBlur={() => setHasFocus(false)}
			onKeyDown={(e) => {
				if (e.key === "Enter") {
					props.save();
				}
			}}
			onInput={(e) => {props.setValue(e.currentTarget.value); setFocusValue(e.currentTarget.value)}}
			readOnly={props.isReadonly}
			disabled={props.isReadonly} 
		/>
	);
}
