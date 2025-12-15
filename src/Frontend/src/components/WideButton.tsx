
type Props = {
	onClick: () => void,
	text: string,
	color: "green" | "red" | "blue" | "purple"
	disabled?: boolean,
}

export function WideButton(props: Props) {
	return (
		<button 
			class={`wide-button ${props.color} margin-right`} 
			onClick={props.onClick} 
			type="button"
			disabled={props.disabled}
		>
			{props.text}
		</button>
	);
}