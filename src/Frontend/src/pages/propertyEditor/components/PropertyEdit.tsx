import { Store } from "../../../utility/propertyStore";
import { FreezeValueButton } from "./FreezeValueButton";
import { useGameProperty } from "../hooks/useGameProperty";
import { Toasts } from "../../../notifications/ToastStore";
import { GamePropertyType } from "pokeaclient";
import { getPropertyFieldValue, PropertyTextbox } from "./PropertyTextbox";
import { PropertyInputSelect } from "./PropertyInputSelect";
import { useEffect, useState } from "preact/hooks";
import { IconButton } from "@/components/IconButton";



export function PropertyEdit({ path }: { path: string }) {
	const property = useGameProperty(path);
	const propertyType = property?.type ?? GamePropertyType.int;
	const reference = property?.reference;
	const isFrozen = property?.isFrozen || false;
	const isReadonly = !!(property?.address === null);
	const type = (propertyType === "bit" || propertyType === "bool") ? "checkbox" : "text";
	const isSelect = reference && reference !== "defaultCharacterMap"
	const actualValue = getPropertyFieldValue(property?.value, propertyType) ?? "";
	const [value, setValue] = useState<string | boolean | null | number[]>(null);
	const [madeEdit, setMadeEdit] = useState(false);
	const [saved, setSaved] = useState(false);
	
	const handleUpdate = (newValue: string | boolean | number[])  => {
		setValue(newValue);
		setMadeEdit(newValue !== actualValue);
	};
	const handleSave = () => {
		let newPropertyValue = value;
		if (property?.type === "byteArray" && typeof(newPropertyValue) === "string") {
			newPropertyValue = newPropertyValue.split(" ").map(x => Number.parseInt(x, 16));
		}
		if (path) {
			Store.client.updatePropertyValue(path, newPropertyValue, isFrozen)
			.then(() => {
				setSaved(true);
				Toasts.push(`Successfully saved property value!`, "task_alt", "green");
			});
		}
	};
	const handleFreeze = async () => {
		if (!isFrozen) {
			const freezeValue = madeEdit ? value : property?.value;
			const message = `Property value now frozen to: '${freezeValue}'!`
			Store.client.updatePropertyValue(path, freezeValue, true)
				.then(() => {
					Toasts.push(message, "task_alt", "green");
				});
			if(madeEdit) {
				setValue(null);
				setMadeEdit(false);
			}
		} else {
			const message = `Property value is no longer frozen!`;
			Store.client.freezeProperty(path, false)
				.then(() => Toasts.push(message, "task_alt", "green"));
		}
	}

	useEffect(() => {
		if (saved) {
			setSaved(false);
			setValue(null);
			setMadeEdit(false);
		}
	}, [property, saved, madeEdit, setSaved, setMadeEdit]);


	let placeholder = "";
	
	if (!madeEdit) {
		if (property?.value === null) {
			placeholder = "null";
		} else if (property?.value === "") {
			placeholder = "empty"
		}
	}
	return (
		<>
			<FreezeValueButton disabled={isReadonly} isFrozen={isFrozen} onClick={handleFreeze} />
			{isSelect
				? <PropertyInputSelect 
					path={path} 
					displayValue={(value ?? "").toString()}
					isReadonly={isReadonly} 
					onChange={handleUpdate}
					placeholder={placeholder}
				/>
				: <PropertyTextbox
					type={type}
					propertyType={propertyType}
					path={path}
					editValue={value}
					save={handleSave}
					setValue={handleUpdate}
					isEdit={madeEdit}
					placeholder={placeholder}
					isReadonly={isReadonly}
				/>
			}
			{ madeEdit && 
				<>
					<IconButton onClick={handleSave} title="Save" icon="save"/>
					<IconButton 
						title="Discard pending changes"
						onClick={() => {setValue(null); setMadeEdit(false)}}
						icon="undo"
					/>
				</>
			}			
		</>
	)
}
