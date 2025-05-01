import { Store } from "../../../utility/propertyStore"
import { SelectInput } from "../../../components/SelectInput";
import { Toasts } from "../../../notifications/ToastStore";
import { useEffect, useState } from "react";
import { useAPI } from "../../../hooks/useAPI";
import { LoadProgress } from "../../../components/LoadProgress";
import { useLocation } from "wouter";
import { AvailableMapper, Mapper } from "pokeaclient";

type MapperSelectProps = {
	mapper: Mapper | null,
	mapperData: AvailableMapper[]
}

export function MapperSelection(props: MapperSelectProps) {
	const { mapper, mapperData } = props;
	const [, setLocation] = useLocation();
	const currentName = mapper?.gameName?.split(" ").slice(1).join(" ").toLocaleLowerCase();
	const changeMapper = useAPI(Store.client.changeMapper);
	const [currentMapper, setCurrentMapper] = useState(
		mapperData?.find(x =>
			x.displayName.toLowerCase().endsWith(currentName ?? "")
		)?.id || null
	);
	const onLoadMapper = () => {
		if (currentMapper) {
			changeMapper.call(currentMapper);
		}
	}
	useEffect(() => {
		if (changeMapper.wasCalled) {
			if (!changeMapper.isLoading && changeMapper.result) {
				Toasts.push("Loaded mapper", "task_alt", "success");
				setLocation("../../properties/");
			} else if (!changeMapper.isLoading && !changeMapper.result) {
				Toasts.push("Failed to load mapper", "", "error");
			}
		}
	}, [setLocation, changeMapper.wasCalled, changeMapper.isLoading, changeMapper.result])

	if (changeMapper.isLoading) {
		return <LoadProgress label="Loading mapper" />
	}
	return (
		<div>
			<span>
				Select the mapper you would like to load:
			</span>
			<br/>
			<SelectInput
				size={35}
				id="mapper-select"
				onSelection={setCurrentMapper}
				value={currentMapper ?? null}
				options={mapperData?.map((x: AvailableMapper) => ({ value: x.id, display: x.displayName })) || []}
			/>
			<div className="margin-top">
				<button className="border-green margin-right" onClick={onLoadMapper}>
					Load Mapper
				</button>
				<button
					className="border-purple"
					onClick={() => Store.client.files.openMapperFolder()}>
					Open Mapper Folder
				</button>
			</div>
		</div>
	);
}