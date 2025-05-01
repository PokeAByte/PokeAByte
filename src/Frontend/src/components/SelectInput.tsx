import React from "react";

type SelectOption<V> = { value: V, display: string }

interface SelectInputProps<V, T extends SelectOption<V>> {
	id: string,
	label?: string,
	value?: V,
	options: T[],
	size?: number,
	onSelection: (value: V) => void,
}

function matchDisplayvalue<T>(search: string) {
	return (option: SelectOption<T>) => {
		if (!search) {
			return true;
		}
		return option.display.toLowerCase().includes(search);
	}
}

function findDisplayByValue<V, T extends SelectOption<V>>(options: T[], value?: V) {
	if (value !== 0 && !value) {
		return "";
	}
	return options.find(x => x.value === value)?.display ?? "";
}

export function SelectInput<Value>(props: SelectInputProps<Value, SelectOption<Value>>) {
	const divRef = React.useRef<HTMLDivElement>(null);
	const [isOpen, setIsOpen] = React.useState<boolean>(false);
	const [searchValue, setSearchValue] = React.useState<string>("");
	const valueDisplay = findDisplayByValue(props.options, props.value);

	const handleSelection = (option: Value) => {
		props.onSelection(option);
		setIsOpen(false);
		setSearchValue("");
	}
	const handleOnFocus = () => {
		setIsOpen(true);
	}
	const handleBlur = () => {
		window.requestAnimationFrame(() => {
			if (divRef.current?.contains(document.activeElement)) {
				console.log("inner");
				return;
			}
			console.log("outer");
			setIsOpen(false);
		});
	}
	const handleKeyDown = (event: React.KeyboardEvent) => {
		console.log(event.key);
		switch (event.key) {
			case "Escape": 
				setIsOpen(false);
				break;
		}
	}

	return (
		<div 
			className={"combobox margin-left " + (isOpen ? "open" : "")} 
			ref={divRef} 
			tabIndex={-1}
			onBlur={handleBlur}
			onKeyDown={handleKeyDown}
		>
			{props.label && <span>{props.label}</span>}
			<input
				size={props.size}
				name={props.id}
				value={isOpen ? searchValue : valueDisplay}
				onFocus={handleOnFocus}
				onBlur={handleBlur}
				onChange={(e) => setSearchValue(e.target.value)}
			/>
			<menu role="combobox">
				{props.options.filter(matchDisplayvalue(searchValue)).map((x, index) =>
					<button role="button" key={index} onClick={() => handleSelection(x.value)}>
						{x.display}
					</button>
				)}
			</menu>
		</div>
	)
}