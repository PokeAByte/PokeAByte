import { Dispatch, StateUpdater, useEffect, useState } from "preact/hooks";
import { WideButton } from "../../../../components/WideButton";
import { ComponentChildren } from "preact";
import { MapperUpdate } from "@/utility/fetch";

type MapperSelectionTableProps = {
	onMapperSelection: Dispatch<StateUpdater<string[]>>,
	availableMappers: MapperUpdate[],
	selectedMappers: string[],
	fallback: ComponentChildren,
	installedHeader?: string
	availableHeader?: string
	onUpdateList?: () => void,
}

export function MapperSelectionTable(props: MapperSelectionTableProps) {
	const { onMapperSelection, availableMappers, selectedMappers } = props;
	const [allMappers, setAllMappers] = useState<MapperUpdate[]>([]);
	const [mappers, setMappers] = useState<MapperUpdate[]>([]);
	const [filter, setFilter] = useState("");
	useEffect(
		() => {
			setMappers(availableMappers)
			setAllMappers(availableMappers)
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
				{props.fallback}
			</>
		)
	}

	return (
		<>
			<div class="flexy-panel">
				<span>
					<label htmlFor="mapper-filter margin-right">Filter:</label>
					<input
						id="mapper-filter"
						type="text"
						class="margin-left"
						placeholder=""
						onInput={(event) => setFilter(event.currentTarget.value)}
					/>
				</span>
				{props.onUpdateList &&
					<WideButton text="Reload mapper list" color="blue" onClick={props.onUpdateList} />
				}
			</div>
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
						{props.installedHeader &&
							<th>{props.installedHeader}</th>
						}
						{props.availableHeader &&
							<th>{props.availableHeader}</th>
						}
					</tr>
				</thead>
				<tbody>
					{mappers.toSorted((a, b) => a.path > b.path ? 1 : -1).map((mapper, i) => {
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
								{props.installedHeader &&
									<td>
										{mapper.version ?? "-"}
									</td>
								}
								{props.availableHeader &&
									<td>
										{mapper.remote_version}
									</td>
								}
							</tr>
						)
					})}
				</tbody>
			</table>
		</>
	)
}