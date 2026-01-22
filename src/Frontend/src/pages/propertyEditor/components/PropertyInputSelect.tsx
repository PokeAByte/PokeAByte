import { SelectOption, SelectInput } from "../../../components/SelectInput";
import { Store } from "../../../utility/propertyStore";


type Props = { 
	path: string, 
	value: string|null,
	reference: string
	isReadonly?: boolean,
	placeholder?: string,
	onChange: (newValue: string|boolean) => void
	save: () => void
}

export function PropertyInputSelect(props: Props) {
	const glossaryItems = Store.getGlossaryItem(props.reference) ?? [];
	const options = glossaryItems
		.map(x => ({ 
			value: x.key, 
			display: x.value
		}));

	const handleSelection = (option: SelectOption<string>) => {
		props.onChange(option.value);
	};

	return (
		<SelectInput<any>
			label=""
			value={props.value}
			options={options}
			isReadonly={props.isReadonly}
			onSave={props.save}
			placeholder={props.placeholder}
			onSelection={handleSelection} 
		/>
	);
}
