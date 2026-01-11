import { Attribution } from "./LicensePage";

/** Renders a simple table with attribution data. See {@link Attribution}.  */
export function AttributionTable(props: { title: string; items: Attribution[]; }) {
	return (
		<>
			<tr class="section"> <td colspan={3}> {props.title}</td> </tr>
			{props.items.map(item => (
				<tr>
					<td>{item.name}</td>
					<td>{item.authors}</td>
					<td>{item.license}</td>
				</tr>
			))}
		</>
	);
}
