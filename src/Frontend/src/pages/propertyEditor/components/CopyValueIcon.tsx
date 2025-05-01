export function CopyValueIcon({ onClick }: { onClick: () => void }) {
	return (
		<button type="button" onClick={onClick}>
			<i className="material-icons"> content_copy </i>
		</button>
	)
}