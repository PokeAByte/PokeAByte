import { useRef, useEffect } from "preact/hooks";

type ModalProps = {
	display: boolean;
	title?: string;
	text: string | React.ReactNode;
	confirmLabel: string;
	/** Callback invoked when the user confirms the action. */
	onConfirm: () => void;
	/** Callback invoked when the user declines / cancels the action. */
	onCancel: () => void;
};

/**
 * A generic modal for confirming or cancelling an action.
 * @param props Component props.
 */
export function ConfirmationModal(props: ModalProps) {
	const dialogRef = useRef<HTMLDialogElement>(null);
	useEffect(
		() => {
			if (!!dialogRef.current && props.display) {
				dialogRef.current.showModal();
			}
		},
		[dialogRef, props.display]
	);

	if (!props.display) {
		return null;
	}
	return (
		<dialog ref={dialogRef} onToggle={(e) => e.newState === "closed" && props.onCancel()}>
			{props.title && <h2>{props.title}</h2>}
			<p>{props.text}</p>
			<div>
				<button class="margin-right" onClick={props.onCancel}>
					CANCEL
				</button>
				<button class="purple" onClick={props.onConfirm}>
					{props.confirmLabel}
				</button>
			</div>
		</dialog>
	);
}
