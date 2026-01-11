
export function Icon(props: {name: string, class?: string}) {
	return (
		<i
			class={"material-icons " + props.class}
			aria-hidden="true"
		>
			{props.name}
		</i>
	)
}