import { useState } from "preact/hooks";
import { useGamePropertyField } from "../hooks/useGamePropertyField";
import { clipboardCopy } from "../utils/clipboardCopy";
import { CopyValueIcon } from "./CopyValueIcon";
import { Store } from "../../../utility/propertyStore";
import { Toasts } from "../../../notifications/ToastStore";
import { IconButton } from "@/components/IconButton";

export function AttributesTable({ path }: { path: string }) {
	const type = useGamePropertyField(path, "type");
	const address = useGamePropertyField(path, "address")?.toString(16);
	const bits = useGamePropertyField(path, "bits");
	const length = useGamePropertyField(path, "length");
	const size = useGamePropertyField(path, "size");
	const reference = useGamePropertyField(path, "reference");
	const memoryContainer = useGamePropertyField(path, "memoryContainer");

	return (
		<table class="attributes-table">
			<tbody>
				<tr>
					<th>type</th>
					<td></td>
					<td>{type}</td>
				</tr>
				<tr>
					<th>length</th>
					<td></td>
					<td>{length}</td>
				</tr>
				{!!size &&
					<tr>
						<th>Size</th>
						<td></td>
						<td>{size}</td>
					</tr>
				}
				<tr>
					<th>path</th>
					<td >
						<CopyValueIcon onClick={() => clipboardCopy(path)} />
					</td>
					<td>{path}</td>
				</tr>
				{!!address &&
					<tr>
						<th>address</th>
						<td >
							<CopyValueIcon onClick={() => clipboardCopy(address ? `${address.toUpperCase()}` : "")} />
						</td>
						<td>{address ? `0x${address.toUpperCase()}` : "-"}</td>
					</tr>
				}
				{!!bits &&
					<tr>
						<th>bits</th>
						<td >
							<CopyValueIcon onClick={() => clipboardCopy(address ? `0x${address.toUpperCase()}` : "")} />
						</td>
						<td>{bits}</td>
					</tr>
				}
				<tr>
					<th>memoryContainer</th>
					<td >
						<CopyValueIcon onClick={() => clipboardCopy(memoryContainer)} />
					</td>
					<td>{memoryContainer ?? "default"}</td>
				</tr>
				{!!reference &&
					<tr>
						<th>reference</th>
						<td >
							<CopyValueIcon onClick={() => clipboardCopy(reference)} />
						</td>
						<td>{reference}</td>
					</tr>
				}
				<tr>
					<th>bytes</th>
					<PropertyByteRow path={path} />
				</tr>
			</tbody>
		</table>
	);
}

const convertByte = (byte: number) =>  byte.toString(16).padStart(2, "0").toUpperCase();
const convertBytes = (bytes: number[] | null) =>  bytes?.map(convertByte) ?? [];

export function PropertyByteRow({ path }: { path: string }) {
	const bytes = useGamePropertyField(path, "bytes");
	const [hasFocus, setHasFocus] = useState<boolean>(false);
	const [madeEdit, setMadeEdit] = useState<boolean>(false);
	const [values, setValues] = useState<string[]>([]);

	const showTrueValues = !hasFocus && !madeEdit;
	const handleEdit = (byte: string, index: number) => {
		const newValues = structuredClone(values);
		newValues[index] = byte.toUpperCase();
		setValues(newValues);
		setMadeEdit(bytes === null || newValues[index] !== convertByte(bytes[index]));
	};
	const handleSave = () => {
		Store.client.updatePropertyBytes(path, values.map(x => parseInt(x, 16)))
			.then(() => {
				setMadeEdit(false);
				Toasts.push(`Successfully saved bytes!`, "task_alt", "green");
			});
		setMadeEdit(false);
	};
	const clipboardBytes = () => {
		const value = showTrueValues 
			? convertBytes(Store.getProperty(path)!.bytes) 
			: values;
		clipboardCopy(value.join(" "));
	};

	if (bytes == null) {
		return null;
	}
	return (
		<>
			<td>
				<CopyValueIcon onClick={clipboardBytes}  />
			</td>
			<td class="property-bytes">
				<span>
					<span>0x&nbsp;</span>
					{(showTrueValues ? (convertBytes(bytes)) : values).map((value, i) => {
						return (
							<input
								key={i}
								type="text"
								size={2}
								maxLength={2}
								value={value}
								onFocus={() => { setHasFocus(true); setValues(convertBytes(bytes)); }}
								onBlur={() => setHasFocus(false)}
								onInput={(e) => handleEdit(e.currentTarget.value, i)}
							/>
						);
					})}
					{madeEdit &&
						<>
							<IconButton onClick={handleSave} title="Save" icon="save"/>
							<IconButton
								disabled={!madeEdit}
								onClick={() => {setValues(convertBytes(bytes)); setMadeEdit(false)}}
								title="Undo"
								icon="undo"
							/>
						</>
					}
				</span>
			</td>
		</>
	);
}