import { useEffect, useState } from "react";
import { MapperUpdate } from "pokeaclient";

type MapperSelectionTableProps = {
	onMapperSelection: React.Dispatch<React.SetStateAction<string[]>>,
	availableMappers: MapperUpdate[],
	selectedMappers: string[],
	onUpdateList?: () => void,
}

export function MapperSelectionTable(props: MapperSelectionTableProps) {
	const { onMapperSelection, availableMappers, selectedMappers } = props;
	const [mappers, setMappers] = useState(availableMappers);
	const [filter, setFilter] = useState("");
	useEffect(
		() => {
			setMappers(availableMappers)
		},
		[availableMappers]
	);

	useEffect(
		() => {
			if (!filter) {
				setMappers(availableMappers);
			} else {
				const filterLower = filter.toLowerCase();
				const filteredMappers = availableMappers
					.filter(x => x.latestVersion.display_name.toLowerCase().includes(filterLower))
				onMapperSelection(selectedMappers.filter(path => filteredMappers.some(filtered => filtered.latestVersion.path === path)));
				setMappers(filteredMappers);
			}
		},
		[filter]
	)

	const selectAll = (checked: boolean) => {
		if (checked) {
			onMapperSelection(mappers.map(x => x.latestVersion.path));
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

	if (mappers.length === 0) {
		return (
			<>
				<br/>
				<strong>No Mappers Found</strong>
			</>
		)
	}

	return (
		<>
			<span>
				<label htmlFor="mapper-filter margin-right">Filter:</label>
				<input 
					id="mapper-filter"
					type="text" 
					className="margin-right"
					placeholder="" 
					onChange={(event) => setFilter(event.target.value)} 
				/>
			</span>
			{props.onUpdateList &&
				<button type="button"  className="border-blue">RELOAD MAPPER LIST</button>
			}
			<table className="striped">
				<thead>
					<tr  >
						<th className="min">
							<label className="checkbox">
								<input type="checkbox" checked={selectedMappers.length == mappers.length} onChange={(e) => selectAll(e.target.checked)} />
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
										onChange={() => { }}
										checked={selectedMappers.includes(mapper.latestVersion.path)}
										onClick={() => select(mapper.latestVersion.path)}
										aria-label="Select mapper"
									/>
								</td>
								<td>
									{mapper.latestVersion.path}
								</td>
							</tr>
						)
					})}
				</tbody>
			</table>
		</>
	)
}