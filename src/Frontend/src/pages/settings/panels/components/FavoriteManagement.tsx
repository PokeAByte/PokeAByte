import { IconButton } from "@/components/IconButton";
import { WideButton } from "@/components/WideButton";
import { mapperFilesSignal } from "@/Contexts/mapperFilesSignal";
import { getFavoriteId, saveSetting, uiSettingsSignal } from "@/Contexts/uiSettingsSignal";
import { MapperFile } from "@/utility/fetch";
import { beautifyMapperName } from "@/utility/mapperName";
import { useComputed } from "@preact/signals";

/** Renders a table of favorite mappers with buttons to change their order or delete them. */
export function FavoriteManagement() {
	const favoriteIds = useComputed(() => uiSettingsSignal.value.favoriteMappers).value ?? [];
	const removeFavorite = (favorite: string) => {
		saveSetting("favoriteMappers", favoriteIds?.filter(x => x !== favorite) ?? []);
	};

	const moveFavorite = (favorite: MapperFile, delta: number) => {
		if (!favoriteIds) {
			return;
		}
		const favoriteId = getFavoriteId(favorite);
		const newArrangement = [...favoriteIds];
		const index = newArrangement.indexOf(favoriteId);
		if (index > 0) {
			newArrangement.splice(index, 1);
			newArrangement.splice(index + delta, 0, favoriteId);
			saveSetting("favoriteMappers", [...newArrangement]);
		}
	}
	const mapperFiles = mapperFilesSignal.value;
	const favorites = favoriteIds?.map(id => mapperFiles.availableMappers?.find(mapper => getFavoriteId(mapper) == id))
		.filter(x => !!x);

	return (
		<tr>
			<th>
				<label>Favorites:</label>
			</th>
			<td>
				<table class="striped">
					<tbody>
						{favorites?.map((favorite, index) => {
							return <tr key={favorite.path}>
								<td>
									<span class="margin-left">{beautifyMapperName(favorite)}</span>
								</td>
								<td>
									<IconButton
										onClick={() => removeFavorite(getFavoriteId(favorite))}
										class="margin-left margin-right text-red"
										icon="delete"
										title="Remove"
									/>
									{index + 1 < favorites.length &&
										<IconButton
											onClick={() => moveFavorite(favorite,1)}
											class="margin-right"
											icon="arrow_downward"
											title="Move down"
										/>
									}
									{index > 0 &&
										<IconButton
											onClick={() => moveFavorite(favorite, -1)}
											class="margin-right float-right"
											icon="arrow_upward"
											title="Move up"
										/>
									}
								</td>
								<br />
							</tr>;
						})}
					</tbody>
				</table>
				{favoriteIds.length > 0 
					? <WideButton text="Clear all" color="red" onClick={() => saveSetting("favoriteMappers", [])} />
					: <span> You currently have no favorites </span>
				}

			</td>
		</tr>
	);
}
