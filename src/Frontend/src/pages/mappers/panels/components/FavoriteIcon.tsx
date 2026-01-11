import { useComputed } from "@preact/signals";
import { saveSetting, uiSettingsSignal } from "../../../../Contexts/uiSettingsSignal";
import { IconButton } from "@/components/IconButton";

export function FavoriteIcon(props: { mapperId: string; }) {
	const favoriteIds = useComputed(() => uiSettingsSignal.value.favoriteMappers).value;
	const handleClick = (event: UIEvent) => {
		if (favoriteIds?.includes(props.mapperId)) {
			saveSetting("favoriteMappers", favoriteIds?.filter(x => x !== props.mapperId) ?? [] );
		} else {
			saveSetting("favoriteMappers", [...(favoriteIds ?? []), props.mapperId]);
		}
		event.stopPropagation();
		return false;
	};

	if (favoriteIds?.includes(props.mapperId)) {
		return (
			<IconButton onClick={handleClick} title="Remove from favorites" class="text-red" icon="favorite" noBorder />
		);
	}
	return (
		<IconButton onClick={handleClick} title="Add to favorites" icon="favorite_border" noBorder />
	);
}