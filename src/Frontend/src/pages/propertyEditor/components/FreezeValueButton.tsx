
export function FreezeValueButton({ isFrozen, onClick, disabled}: { isFrozen: boolean, onClick: () => void, disabled: boolean }) {

	const classes = isFrozen ? "icon-button margin-right blue highlight" : "icon-button margin-right";
	return (
		<button className={classes} type="button" onClick={onClick} disabled={disabled} title={"Freeze (reset value if it changes)"}>
			<i className="material-icons"> ac_unit </i>
		</button>
	)
}
