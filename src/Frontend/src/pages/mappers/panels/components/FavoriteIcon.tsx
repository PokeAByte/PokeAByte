import { useComputed } from "@preact/signals";
import { getFavoriteId, saveSetting, uiSettingsSignal } from "../../../../Contexts/uiSettingsSignal";
import { IconButton } from "@/components/IconButton";
import { MapperFile } from "@/utility/fetch";


export function FavoriteIcon(props: { mapper: MapperFile; }) {
	const favoriteIds = useComputed(() => uiSettingsSignal.value.favoriteMappers).value;
	const id = getFavoriteId(props.mapper);
	const handleClick = (event: UIEvent) => {
		if (favoriteIds?.includes(id)) {
			saveSetting("favoriteMappers", favoriteIds?.filter(x => x !== id) ?? [] );
		} else {
			saveSetting("favoriteMappers", [...(favoriteIds ?? []), id]);
		}
		event.stopPropagation();
		return false;
	};

	if (favoriteIds?.includes(id)) {
		return (
			<IconButton onClick={handleClick} title="Remove from favorites" class="text-red" icon="favorite" noBorder />
		);
	}
	return (
		<IconButton onClick={handleClick} title="Add to favorites" icon="favorite_border" noBorder />
	);
}