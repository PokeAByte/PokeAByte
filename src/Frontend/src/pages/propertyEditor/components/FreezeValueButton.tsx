import { IconButton } from "@/components/IconButton";

export function FreezeValueButton({ isFrozen, onClick, disabled}: { isFrozen: boolean, onClick: () => void, disabled: boolean }) {
	const classes = isFrozen ? "text-blue" : "";
	return (
		<IconButton 
			class={classes} 
			onClick={onClick} 
			disabled={disabled} 
			title="Freeze (reset value if it changes)"
			icon="ac_unit"
		/>
	)
}
