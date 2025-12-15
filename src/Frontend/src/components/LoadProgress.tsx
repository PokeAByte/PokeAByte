/**
 * Renders spinning progress bar with the given label.
 */
export function LoadProgress({ label }: { label: string }) {
	return (
		<>
			<h2>
				{label}
			</h2>
			<div>
				<progress class="error" />
			</div>
		</>
	);
}