
type ButtonProps = {
	color?: string,
	onClick?: () => void,
	disabled?: boolean,
	small?: boolean
	type?: "button" | "submit" | "reset",
} & React.PropsWithChildren

export function Button(props: ButtonProps) {
	return (
		<button
			onClick={props.onClick}
			disabled={props.disabled}
			type={props.type ?? "button"}
		>
			{props.children}
		</button>
	)
}