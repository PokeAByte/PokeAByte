import { useContext } from "preact/hooks";
import { MapperFilesContext } from "@/Contexts/availableMapperContext";
import { useUISetting } from "@/Contexts/UISettingsContext";
import { useAPI } from "@/hooks/useAPI";
import { changeMapper } from "@/utility/fetch";
import { Panel } from "../../../components/Panel";
import { createMapperLoadToast } from "./createMapperLoadToast";
import { CSSProperties } from "preact";
import { getMapperColors } from "@/utility/getMapperColors";

export function RecentPanel() {
	const [isEnabled] = useUISetting("recentlyUsedEnabled");
	const [recentMappers] = useUISetting("recentMappers");
	const mapperFileContext = useContext(MapperFilesContext);
	const changeMapperApi = useAPI(changeMapper, createMapperLoadToast);
	const mappers = recentMappers?.map(id => mapperFileContext.availableMappers?.find(mapper => mapper.id == id))
		.filter(x => !!x);

	if (!isEnabled || !mappers?.length) {
		return null;
	}

	return (
		<Panel id="mapper-recent" title="Recently used mappers" defaultOpen>
			<div class="favorites">
				{mappers?.map((favorite) => {
					const buttonColors = getMapperColors(favorite.displayName);
					const style: CSSProperties = {};
					if (buttonColors) {
						style.borderColor = buttonColors.border;
						style.backgroundColor = buttonColors.bg;
						style.color = buttonColors.text;
					}
					return (
						<button
							class="margin-right"
							onClick={() => changeMapperApi.call(favorite.id)}
							style={style}
						>
							{favorite.displayName}
						</button>
					);
				})}
			</div>
		</Panel>
	);
}