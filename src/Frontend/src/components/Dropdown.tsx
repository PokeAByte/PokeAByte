import { useRef, useState } from "preact/hooks";
import { SelectInputProps, SelectOption, findDisplayByValue } from "./SelectInput";

export function Dropdown<Value>(props: SelectInputProps<Value, SelectOption<Value>>) {
	const divRef = useRef<HTMLDivElement>(null);
	const [isOpen, setIsOpen] = useState<boolean>(false);
	const valueDisplay = findDisplayByValue(props.options, props.value);

	const handleSelection = (option: SelectOption<Value>) => {
		props.onSelection(option);
		setIsOpen(false);
	};
	const handleOnFocus = () => {
		setIsOpen(!props.isReadonly);
	};
	const handleBlur = () => {
		window.requestAnimationFrame(() => {
			if (divRef.current?.contains(document.activeElement)) {
				return;
			}
			setIsOpen(false);
		});
	};
	const handleKeyDown = (event: KeyboardEvent) => {
		switch (event.key) {
			case "Escape":
				setIsOpen(false);
				break;
		}
	};

	return (
		<div
			className={"combobox " + (isOpen ? "open" : "")}
			ref={divRef}
			tabIndex={-1}
			onBlur={handleBlur}
			onKeyDown={handleKeyDown}
		>
			{props.label && <span>{props.label}</span>}
			<input
				tabIndex={props.tabIndex}
				autocomplete="off"
				autocorrect="off"
				readonly
				size={props.size}
				name={props.id}
				value={valueDisplay}
				onFocus={handleOnFocus}
				onBlur={handleBlur}
				readOnly={props.isReadonly} />
			<menu role="combobox">
				<div>
					{props.options.map((x, index) => <button role="button" key={index} onClick={() => handleSelection(x)}>
						{x.display}
					</button>
					)}
				</div>
			</menu>
		</div>
	);
}
