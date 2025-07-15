import { GamePropertyType } from "pokeaclient";
import { useGamePropertyField } from "../hooks/useGamePropertyField";
import { useState } from "preact/hooks";

export function getPropertyFieldValue(value: any, type: GamePropertyType | null) {
	if (type === "bitArray") {
		return value.map((x: boolean) => x ? "1" : "0").join("") ?? "";
	}
	return value;
}

export type PropertyTextboxProps = {
	path: string,
	type: string,
	isEdit: boolean,
	isReadonly: boolean,
	propertyType: GamePropertyType | null,
	editValue: string|boolean | null,
	placeholder?: string,
	save: () => void,
	setValue: (value: string|boolean) => void,
}

export function PropertyTextbox(props: PropertyTextboxProps) {
	const propertyValue = useGamePropertyField(props.path, "value");
	const value = getPropertyFieldValue(propertyValue, props.propertyType) ?? "";
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
					type={"checkbox"}
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
