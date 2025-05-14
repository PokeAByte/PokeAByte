export function SaveValueButton({ active, onClick }: { active: boolean, onClick: () => void }) {
	return (
		<button 
			className="icon-button margin-right" 
			disabled={!active} 
			type="button" 
			onClick={() => active && onClick()}
			title={"Save"}
		>
			<i className="material-icons"> save </i>
		</button>
	)
}
