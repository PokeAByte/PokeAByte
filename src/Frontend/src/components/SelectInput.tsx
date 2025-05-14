import { useRef, useState } from "preact/hooks";

export type SelectOption<V> = { value: V, display: string }

export interface SelectInputProps<V, T extends SelectOption<V>> {
	id: string,
	label?: string,
	value?: V,
	options: T[],
	isReadonly?: boolean,
	size?: number,
	placeholder?: string,
	tabIndex?: number
	onSelection: (value: SelectOption<V>) => void,
}

function matchDisplayValue<T>(search: string) {
	return (option: SelectOption<T>) => {
		if (!search) {
			return true;
		}
		return option.display.toString().toLowerCase().includes(search);
	}
}

export function findDisplayByValue<V, T extends SelectOption<V>>(options: T[], value?: V) {
	if (value !== 0 && value !== "" && !value) {
		return "";
	}
	return options.find(x => x.value === value)?.display ?? "";
}

export function SelectInput<Value>(props: SelectInputProps<Value, SelectOption<Value>>) {
	const divRef = useRef<HTMLDivElement>(null);
	const [isOpen, setIsOpen] = useState<boolean>(false);
	const [searchValue, setSearchValue] = useState<string>("");
	const [focusIndex, setFocusedIndex] = useState<number>(-1);
	const valueDisplay = findDisplayByValue(props.options, props.value);
	const filteredOptions = props.options.filter(matchDisplayValue(searchValue));
	const optionRefs = useRef<Array<HTMLButtonElement | null>>([]);
	const optionsContainer = useRef<HTMLDivElement | null>(null);
	const inputRef = useRef<HTMLInputElement | null>(null);

	const handleSelection = (option: SelectOption<Value>) => {
		props.onSelection(option);
		setIsOpen(false);
		setFocusedIndex(-1);
		setSearchValue("");
	}
	const handleOnFocus = () => {
		setIsOpen(!props.isReadonly);		
	}
	const handleBlur = () => {
		window.requestAnimationFrame(() => {
			if (divRef.current?.contains(document.activeElement)) {
				return;
			}
			setIsOpen(false);
			setFocusedIndex(-1);
		});
	}
	const handleKeyDown = (event: KeyboardEvent) => {
		let newFocus = -1;
		if (!isOpen) {
			return;
		}
		switch (event.key) {
			case "ArrowDown":
				event.preventDefault();
				newFocus = focusIndex < (filteredOptions.length - 1) ? focusIndex + 1 : 0;
				(optionsContainer.current?.childNodes[newFocus] as HTMLElement)?.focus();
				setFocusedIndex(focusIndex < (filteredOptions.length - 1) ? focusIndex + 1 : 0);
				break;
			case "ArrowUp":
				event.preventDefault();
				newFocus = focusIndex > 0 ? focusIndex - 1 : filteredOptions.length - 1;
				(optionsContainer.current?.childNodes[newFocus] as HTMLElement)?.focus();
				setFocusedIndex(focusIndex > 0 ? focusIndex - 1 : filteredOptions.length - 1);
				break;
			case "Escape":
				event.preventDefault();
				setFocusedIndex(-1);
				inputRef.current?.focus();
				break;
		}
	}

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
				autocomplete="off"
				autocorrect="off"
				size={props.size}
				name={props.id}
				value={isOpen ? searchValue : valueDisplay}
				onFocus={handleOnFocus}
				onBlur={handleBlur}
				placeholder={props.placeholder}
				onInput={(e) => setSearchValue(e.currentTarget.value)}
				readOnly={props.isReadonly}
				ref={inputRef}
			/>
			<menu role="combobox">
				<div ref={optionsContainer}>
					{filteredOptions.map((x, index) =>
						<button
							role="button"
							key={index}
							ref={el => optionRefs.current[index] = el}
							onClick={() => handleSelection(x)}
							tabIndex={-1}
							className={focusIndex === index ? "focused" : ""}
						>
							{x.display}
						</button>
					)}
				</div>
			</menu>
		</div>
	)
}
