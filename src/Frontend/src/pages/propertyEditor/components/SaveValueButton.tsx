import { IconButton } from "@/components/IconButton";

export function SaveValueButton({ onClick }: { onClick: () => void }) {
	return (
		<IconButton onClick={onClick} title="Save" icon="save"/>
	)
}
