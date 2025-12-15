import { MapperUpdate, MapperVersion } from "pokeaclient";
import { useEffect, useState } from "preact/hooks";
import { WideButton } from "../../../../components/WideButton";

type MapperSelectionTableProps = {
	onMapperSelection: React.Dispatch<React.SetStateAction<string[]>>,
	availableMappers: MapperUpdate[],
	selectedMappers: string[],
	onUpdateList?: () => void,
}

export function MapperSelectionTable(props: MapperSelectionTableProps) {
	const { onMapperSelection, availableMappers, selectedMappers } = props;
	const [allMappers, setAllMappers] = useState<MapperVersion[]>([]);
	const [mappers, setMappers] = useState<MapperVersion[]>([]);
	const [filter, setFilter] = useState("");
	useEffect(
		() => {
			setMappers(availableMappers.map(x => x.currentVersion ?? x.latestVersion).filter(x => !!x))
			setAllMappers(availableMappers.map(x => x.currentVersion ?? x.latestVersion).filter(x => !!x))
		},
		[availableMappers, setMappers, setAllMappers]
	);

	useEffect(
		() => {
			if (!filter) {
				setMappers(allMappers);
			} else {
				const filterLower = filter.toLowerCase();
				const filteredMappers = allMappers
					.filter(
						x => x.display_name.toLowerCase().includes(filterLower)
							|| x.path.toLowerCase().includes(filterLower)
					);
				setMappers(filteredMappers);
			}
		},
		[filter, allMappers, onMapperSelection, selectedMappers]
	)

	const selectAll = (checked: boolean) => {
		if (checked) {
			onMapperSelection(mappers.map(x => x.path));
		} else {
			onMapperSelection([]);
		}
	}

	const select = (path: string) => {
		if (selectedMappers.includes(path)) {
			onMapperSelection(selectedMappers.filter(x => x != path))
		} else {
			onMapperSelection([...selectedMappers, path]);
		}
	}

	if (allMappers.length === 0) {
		return (
			<>
				<br />
				<strong>No Mappers Found</strong>
			</>
		)
	}

	return (
		<>
			<label htmlFor="mapper-filter margin-right">Filter:</label>
			<input
				id="mapper-filter"
				type="text"
				className="margin-right margin-left"
				placeholder=""
				onInput={(event) => setFilter(event.currentTarget.value)}
			/>
			{props.onUpdateList &&
				<WideButton text="Reload mapper list" color="blue" onClick={props.onUpdateList} />
			}
			<table class="striped">
				<thead>
					<tr  >
						<th>
							<label class="checkbox">
								<input  
									type="checkbox"  
									checked={selectedMappers.length == mappers.length}  
									onInput={(e) => selectAll(e.currentTarget.checked)}  
								/>
								<span />
							</label>
						</th>
						<th> Path </th>
					</tr>
				</thead>
				<tbody>
					{mappers.map((mapper, i) => {
						return (
							<tr key={i}>
								<td >
									<input
										type="checkbox"
										onInput={() => { }}
										checked={selectedMappers.includes(mapper.path)}
										onClick={() => select(mapper.path)}
										aria-label="Select mapper"
									/>
								</td>
								<td>
									{mapper.path}
								</td>
							</tr>
						)
					})}
				</tbody>
			</table>
		</>
	)
}