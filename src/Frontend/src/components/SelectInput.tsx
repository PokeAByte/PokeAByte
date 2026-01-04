import { useRef, useState } from "preact/hooks";

export type SelectOption<V> = { value: V, display: string, extra?: React.ReactNode }

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
		search = search.toLowerCase();
		return option.display.toString().toLowerCase().includes(search);
	}
}

export function findDisplayByValue<V, T extends SelectOption<V>>(options: T[], value?: V) {
	if (value === null || value === undefined) {
		return "";
	}
	return options.find(x => x.value === value)?.display ?? "";
}

function wrapIndex(index: number, length: number) {
	if (index < 0) {
		return length-1;
	}
	if (index == length) {
		return 0;
	}
	return index;
}

export function SelectInput<Value>(props: SelectInputProps<Value, SelectOption<Value>>) {
	const divRef = useRef<HTMLDivElement>(null);
	const [isOpen, setIsOpen] = useState<boolean>(false);
	const [searchValue, setSearchValue] = useState<string>("");
	const [focusIndex, setFocusedIndex] = useState<number>(-1);
	const valueDisplay = findDisplayByValue(props.options, props.value);
	const filteredOptions = props.options.filter(matchDisplayValue(searchValue));
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
		let focusShift = 0;
		switch (event.key) {
			case "Space":
			case "Enter":
				if (filteredOptions.length > 0 && focusIndex === -1) {
					handleSelection(filteredOptions[0]);
				}
				break;
			case "Tab":
				focusShift = event.shiftKey 
					? focusShift = -1
					: focusShift = 1;
				break;
			case "ArrowDown":
				focusShift = 1;
				break;
			case "ArrowUp":
				focusShift = -1;
				break;
			case "Escape":
				inputRef.current?.blur();
				(optionsContainer.current?.childNodes[focusIndex] as HTMLElement)?.blur();
				break;
		}
		if (focusShift != 0) {
			event.preventDefault();
			newFocus = wrapIndex(focusIndex + focusShift, filteredOptions.length);
			(optionsContainer.current?.childNodes[newFocus] as HTMLElement)?.focus();
			setFocusedIndex(newFocus);
		}
	}

	return (
		<div
			class={"combobox " + (isOpen ? "open" : "")}
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
							onClick={() => handleSelection(x)}
							class={index === 0 && focusIndex === -1 ? "text-blue" : ""}
							tabIndex={-1}
						>
							<span>
								{x.display}
							</span>
							{x.extra && 
								<span>{x.extra}</span>
							}
						</button>
					)}
				</div>
			</menu>
		</div>
	)
}
