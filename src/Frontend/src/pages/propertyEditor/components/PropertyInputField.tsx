import React from "react";
import { Store } from "../../../utility/propertyStore";
import { SaveValueButton } from "./SaveValueButton";
import { FreezeValueButton } from "./FreezeValueButton";
import { useGameProperty } from "../hooks/useGameProperty";
import { Toasts } from "../../../notifications/ToastStore";
import { SelectInput } from "../../../components/SelectInput";
import { useGamePropertyField } from "../hooks/useGamePropertyField";
import { GamePropertyType } from "pokeaclient";

export function getPropertyFieldValue(value: any, type: GamePropertyType | null) {
	if (type === "bitArray") {
		return value.map((x: boolean) => x ? "1" : "0").join("") ?? "";
	}
	return value;
}

export function PropertyInputField({ path }: { path: string }) {
	const propertyType = useGamePropertyField(path, "type");
	const reference = useGamePropertyField(path, "reference");
	const isFrozen = useGamePropertyField(path, "isFrozen") || false;
	const type = (propertyType === "bit" || propertyType === "bool") ? "checkbox" : "text";
	const isSelect = reference && reference !== "defaultCharacterMap"
	const [value, setValue] = React.useState<string>("");
	const [hasFocus, setHasFocus] = React.useState(false);
	const [madeEdit, setMadeEdit] = React.useState(false);
	const handleSave = () => {
		if (path) {
			Store.client.updatePropertyValue(path, value)
				.then(() => {
					setMadeEdit(false);
					Toasts.push(`Saved successful`, "task_alt", "success");
				});
		}
	};


	return (
		<>
			{isSelect
				? <PropertyInputSelect path={path} />
				: <PropertyTextbox
					type={type}
					propertyType={propertyType}
					path={path}
					editValue={(hasFocus || madeEdit) ? value : null}
					setHasFocus={setHasFocus}
					save={handleSave}
					setValue={setValue}
					isEdit={hasFocus || madeEdit}
					setMadeEdit={setMadeEdit}
				/>
			}
			<SaveValueButton active={madeEdit} onClick={handleSave} />
			<FreezeValueButton isFrozen={isFrozen} path={path} />
		</>
	)
}

type PropertyTextboxProps = {
	path: string,
	type: string,
	isEdit: boolean,
	propertyType: GamePropertyType | null,
	editValue: string | null,
	save: () => void,
	setHasFocus: (focus: boolean) => void,
	setValue: (value: string) => void,
	setMadeEdit: (edited: boolean) => void,
}

export function PropertyTextbox(props: PropertyTextboxProps) {
	const propertyValue = useGamePropertyField(props.path, "value");
	const value = getPropertyFieldValue(propertyValue, props.propertyType) ?? "";
	if (props.type === "checkbox") {
		return (
			<label>
				<input
					type={"checkbox"}
					role="switch"
					value={props.isEdit ? props.editValue : value}
					onFocus={() => { props.setHasFocus(true); props.setValue(propertyValue); }}
					onBlur={() => props.setHasFocus(false)}
					onKeyDown={(e) => {
						if (e.key === "Enter") {
							props.save();
						}
					}}
					onChange={(e) => { props.setValue(e.currentTarget.value); props.setMadeEdit(true) }}
				/>
			</label>
		);
	}
	return (
		<input
			className="margin-left"
			type={props.type}
			value={props.isEdit ? props.editValue : value}
			onFocus={() => { props.setHasFocus(true); props.setValue(propertyValue); }}
			onBlur={() => props.setHasFocus(false)}
			onKeyDown={(e) => {
				if (e.key === "Enter") {
					props.save();
				}
			}}
			onChange={(e) => { props.setValue(e.currentTarget.value); props.setMadeEdit(true) }}
		/>
	)
}
export function PropertyInputSelect({ path }: { path: string }) {
	const property = useGameProperty(path);
	const glossaryItems = Store.getGlossaryItem(property!.reference!) ?? [];
	const options = glossaryItems
		.filter((x) => x.value)
		.map(x => ({ value: x.key, display: x.value }));

	const value = glossaryItems.find(x => x.value === property!.value)?.key;
	return (
		<>
			<SelectInput
				label=""
				id={`${property!.path}-input`}
				value={value}
				options={options}
				onSelection={(e) => console.log(e)}
			/>
		</>
	)
}