import { SelectOption, SelectInput } from "../../../components/SelectInput";
import { Store } from "../../../utility/propertyStore";
import { useGameProperty } from "../hooks/useGameProperty";


type Props = { 
	path: string, 
	displayValue: string,
	isReadonly?: boolean,
	placeholder?: string,
	onChange: (newValue: string|boolean) => void
	save: () => void
}

export function PropertyInputSelect({ path, isReadonly, onChange, displayValue, placeholder }: Props) {
	const property = useGameProperty(path);
	const glossaryItems = Store.getGlossaryItem(property!.reference!) ?? [];
	const options: SelectOption<string>[] = glossaryItems
		.map(x => ({ 
			value: x.key.toString(), 
			display: x.value === null ? `[${x.key.toString()}]` : x.value
		}));

	const value = !displayValue
		? glossaryItems.find(x => x.value === property!.value)?.key
		: glossaryItems.find(x => x.value?.toString() === displayValue)?.key;

	const handleSelection = (option: SelectOption<string>) => {
		onChange(option.display);
	};

	return (
		<SelectInput
			label=""
			id={`${property!.path}-input`}
			value={value?.toString()}
			options={options}
			isReadonly={isReadonly}
			placeholder={placeholder}
			onSelection={handleSelection} 
		/>
	);
}
