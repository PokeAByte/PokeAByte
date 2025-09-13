import { useRef, useEffect } from "preact/hooks";

type ModalProps = {
	display: boolean;
	title?: string;
	text: string | React.ReactNode;
	confirmLabel: string;
	onConfirm: () => void;
	onCancel: () => void;
};

export function ConfirmationModal(props: ModalProps) {
	const dialogRef = useRef<HTMLDialogElement>(null);
	const hasCurrentDialog = !!dialogRef.current;
	useEffect(() => {
		if (!!dialogRef.current && props.display) {
			dialogRef.current.showModal();
		}
	}, [hasCurrentDialog, props.display]);
	if (!props.display) {
		return null;
	}
	return (
		<dialog ref={dialogRef} onToggle={(e) => e.newState === "closed" && props.onCancel()}>
			{props.title && <h2>{props.title}</h2>}
			<p>{props.text}</p>
			<div>
				<button className="margin-right" onClick={props.onCancel}>
					CANCEL
				</button>
				<button className="purple" onClick={props.onConfirm}>
					{props.confirmLabel}
				</button>
			</div>
		</dialog>
	);
}
