import { SelectOption, SelectInput } from "../../../components/SelectInput";
import { Store } from "../../../utility/propertyStore";
import { useGameProperty } from "../hooks/useGameProperty";


type Props = { 
	path: string, 
	displayValue: string,
	isReadonly?: boolean,
	placeholder?: string,
	onChange: (newValue: string|boolean) => void
}

export function PropertyInputSelect({ path, isReadonly, onChange, displayValue, placeholder }: Props) {
	const property = useGameProperty(path);
	const glossaryItems = Store.getGlossaryItem(property!.reference!) ?? [];
	const options = glossaryItems
		.filter((x) => x.value)
		.map(x => ({ value: x.key.toString(), display: x.value }));

	const value = !displayValue 
		? glossaryItems.find(x => x.value === property!.value)?.key
		: glossaryItems.find(x => x.value === displayValue)?.key;

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
			onSelection={handleSelection} />
	);
}
