
type Props = {
	/** Called when the button is clicked. */
	onClick: () => void,
	/** The label / text of the button */
	text: string,
	/** Color of the button. */
	color: "green" | "red" | "blue" | "purple"
	/** Whether the button is disabled / non-interactive. */
	disabled?: boolean,
}

/** A button with a minimum width.  */
export function WideButton(props: Props) {
	return (
		<button 
			class={`wide-button ${props.color}`} 
			onClick={props.onClick} 
			type="button"
			disabled={props.disabled}
		>
			{props.text}
		</button>
	);
}