import { Store } from "../../../utility/propertyStore";
import { SaveValueButton } from "./SaveValueButton";
import { FreezeValueButton } from "./FreezeValueButton";
import { useGameProperty } from "../hooks/useGameProperty";
import { Toasts } from "../../../notifications/ToastStore";
import { GamePropertyType } from "pokeaclient";
import { getPropertyFieldValue, PropertyTextbox } from "./PropertyTextbox";
import { PropertyInputSelect } from "./PropertyInputSelect";
import { useEffect, useState } from "preact/hooks";
import { CopyValueIcon } from "./CopyValueIcon";
import { clipboardCopy } from "../utils/clipboardCopy";
import { Advanced } from "../../../Contexts/Advanced";
import { ToggleHidden } from "../../../Contexts/HidePropertyContext";

export function PropertyEdit({ path }: { path: string }) {
	const property = useGameProperty(path);
	const propertyType = property?.type ?? GamePropertyType.int;
	const reference = property?.reference;
	const isFrozen = property?.isFrozen || false;
	const isReadonly = !!(property?.address === null);
	const type = (propertyType === "bit" || propertyType === "bool") ? "checkbox" : "text";
	const isSelect = reference && reference !== "defaultCharacterMap"
	const actualValue = getPropertyFieldValue(property?.value, propertyType) ?? "";
	const [value, setValue] = useState<string|boolean|null>(null);
	const [madeEdit, setMadeEdit] = useState(false);
	const [saved, setSaved] = useState(false);
	const handleUpdate = (newValue: string|boolean)  => {
		setValue(newValue);
		setMadeEdit(newValue !== actualValue);
	};
	const handleSave = () => {
		if (path) {
			Store.client.updatePropertyValue(path, value, isFrozen)
			.then(() => {
				setSaved(true);
				Toasts.push(`Successfully saved property value!`, "task_alt", "success");
			});
		}
	};
	const handleFreeze = async () => {
		if (!isFrozen) {
			const freezeValue = madeEdit ? value : property?.value;
			const message = `Property value now frozen to: '${freezeValue}'!`
			Store.client.updatePropertyValue(path, freezeValue, true)
				.then(() => {
					Toasts.push(message, "task_alt", "success");
				});
			if(madeEdit) {
				setValue(null);
				setMadeEdit(false);
			}
		} else {
			const message = `Property value is no longer frozen!`;
			Store.client.freezeProperty(path, false)
				.then(() => Toasts.push(message, "task_alt", "success"));
		}
	}
	const handleCopyClick = () => {
		const currentPropValue = Store.getProperty(path);
		if (currentPropValue) {
			clipboardCopy(getPropertyFieldValue(property?.value, propertyType));
		}
	};
	useEffect(() => {
		if (saved) {
			setSaved(false);
			setMadeEdit(false);
		}
	}, [property, saved, madeEdit, setSaved, setMadeEdit]);

	let addressString = property?.address ? `0x${property?.address.toString(16).toUpperCase()}` : "";
	if (property?.bits) {
		addressString += property.bits.includes("-")
			? ` (bits: ${property.bits})`
			: ` (bit: ${property.bits})`
	}
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
			<CopyValueIcon onClick={handleCopyClick} />
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
					<SaveValueButton active={madeEdit} onClick={handleSave} />
					<button 
						className="icon-button margin-left" 
						disabled={!madeEdit} 
						type="button" 
						title={"Discard pending changes"}
						onClick={() => {setValue(null); setMadeEdit(false)}}
						>
						<i className="material-icons"> undo </i>
					</button>
				</>
			}
			<Advanced>
				<span class="margin-left color-darker center-self">
					{addressString}
				</span>
			</Advanced>
			<Advanced>
				<ToggleHidden path={path} />
			</Advanced>
		</>
	)
}
