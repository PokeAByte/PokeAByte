import { MapperArchive } from "@/api/types";

export function ArchivedMapperListItem({ item }: { item: MapperArchive; }) {
	return item.mapper.version
		? (
			<li>
				{item.mapper.path}/{item.mapper.display_name}
				&nbsp;
				(<i>{item.mapper.version}</i>)
			</li>
		)
		: <li>{item.mapper.path}/{item.mapper.display_name}</li>;
}
