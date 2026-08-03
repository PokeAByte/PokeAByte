import { ComponentChild, TargetedToggleEvent } from "preact";
import { useRef, useEffect } from "preact/hooks";

type ModalProps = {
	/** Whether or not to display the modal / dialog. */
	display: boolean;
	/** The title of the modal. */
	title?: string;
	/** The text / content of the modal. */
	children: ComponentChild;
	/** The label for the confirmation button. */
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

	const onToggle = (event: TargetedToggleEvent<HTMLDialogElement>) => {
		if (event.newState === "closed") {
			props.onCancel();
		}
	}

	if (!props.display) {
		return null;
	}
	return (
		<dialog ref={dialogRef} onToggle={onToggle}>
			{props.title && <h2>{props.title}</h2>}
			<div>
				{props.children}
			</div>
			<div class="buttons">
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
