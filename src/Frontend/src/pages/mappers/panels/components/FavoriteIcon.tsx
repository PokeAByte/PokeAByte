import { useUISetting } from "../../../../Contexts/UISettingsContext";
import { IconButton } from "@/components/IconButton";

export function FavoriteIcon(props: { mapperId: string; }) {
	const [favoriteIds, setFavorites] = useUISetting("favoriteMappers");
	const handleClick = (event: UIEvent) => {
		if (favoriteIds?.includes(props.mapperId)) {
			setFavorites(favoriteIds?.filter(x => x !== props.mapperId) ?? []);
		} else {
			setFavorites([...(favoriteIds ?? []), props.mapperId]);
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