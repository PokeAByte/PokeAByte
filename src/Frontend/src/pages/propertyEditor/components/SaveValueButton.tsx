export function SaveValueButton({ active, onClick }: { active: boolean, onClick: () => void }) {
	return (
		<button className="margin-left" disabled={!active} type="button" onClick={() => active && onClick()}>
			<i className="material-icons"> save </i>
		</button>
	)
}
