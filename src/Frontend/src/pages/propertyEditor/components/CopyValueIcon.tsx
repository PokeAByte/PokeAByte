import { IconButton } from "@/components/IconButton";

export function CopyValueIcon({ onClick }: { onClick: () => void }) {
	return (
		<IconButton
			onClick={onClick}
			title="Copy value"
			icon="content_copy"
		/>
	)
}