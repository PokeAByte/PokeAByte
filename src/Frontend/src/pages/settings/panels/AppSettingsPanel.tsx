
import { useEffect, useState } from "preact/hooks";
import { ConfirmationModal } from "@/components/ConfirmationModal";
import { useAPI } from "@/hooks/useAPI";
import { AppSettingsModel, getAppSettings, resetAppSettings, saveAppSettings } from "@/utility/fetch";
import { Toasts } from "@/notifications/ToastStore";
import { TargetedInputEvent } from "preact";
import { Panel } from "@/components/Panel";
import { WideButton } from "@/components/WideButton";

export function AppSettingsPanel() {
	const [dialog, setDialog] = useState<boolean>(false);
	const [formState, setFormState] = useState<Partial<AppSettingsModel>>({});
	const generalSettings = useAPI(
		getAppSettings,
		(_, result) => {
			if (result != null) {
				setFormState(result as AppSettingsModel);
			}
		}
	);
	const doReset = useAPI(resetAppSettings, () => {
		generalSettings.call();
		setDialog(false);
	})
	useEffect(
		() => generalSettings.call(),
		// eslint-disable-next-line react-hooks/exhaustive-deps
		[]
	);
	const onSubmit = (event: any) => {
		saveAppSettings(formState).then((ok) => {
			if (ok) {
				Toasts.push("Settings saved", "", "green");
			} else {
				Toasts.push("Failed to save settings.", "", "red");
			}
		});
		event.preventDefault();
		return false;
	}
	const setField = (e: TargetedInputEvent<HTMLInputElement>) => {
		if (e.currentTarget) {
			setFormState({
				...formState,
				[e.currentTarget.name]: e.currentTarget.value
			});
		}
	}
	return (
		<Panel id="settings_general" title="Poke-A-Byte settings">
			<span class="text-red">
				<strong>
					Changes will not apply immediately. You have to reload the current mapper.
				</strong>
			</span>
			<hr />
			<form onSubmit={onSubmit}>
				<table>
					<tbody>
						<tr>
							<th>
								<label htmlFor="owner">RetroArch IP: </label>
							</th>
							<td>
								<input
									type="text"
									name="RETROARCH_LISTEN_IP_ADDRESS"
									value={formState.RETROARCH_LISTEN_IP_ADDRESS}
									onInput={setField}
								/>
							</td>
						</tr>
						<tr>
							<th>
								<label htmlFor="owner">RetroArch port: </label>
							</th>
							<td>
								<input
									type="text"
									name="RETROARCH_LISTEN_PORT"
									value={formState.RETROARCH_LISTEN_PORT}
									onInput={setField}
								/>
							</td>
						</tr>
						<tr>
							<th>
								<label htmlFor="owner">RetroArch timeout (ms): </label>
							</th>
							<td>
								<input
									type="text"
									name="RETROARCH_READ_PACKET_TIMEOUT_MS"
									value={formState.RETROARCH_READ_PACKET_TIMEOUT_MS}
									onInput={setField}
								/>
							</td>
						</tr>
						<tr>
							<th>
								<label htmlFor="DELAY_MS_BETWEEN_READS">Polling interval (ms): </label>
							</th>
							<td>
								<input
									type="text"
									name="DELAY_MS_BETWEEN_READS"
									value={formState.DELAY_MS_BETWEEN_READS}
									onInput={setField}
									aria-describedby="delay_description"
								/>
								<p id="delay_description">
									How long Poke-A-Byte waits between reading memory from the emulator. <br />
									Recommended minimum value: 5 ms.
								</p>
							</td>
						</tr>
						<tr>
							<th>
								<label htmlFor="PROTOCOL_FRAMESKIP">EDP frameskip: </label>
							</th>
							<td>
								<input
									type="text"
									name="PROTOCOL_FRAMESKIP"
									value={formState.PROTOCOL_FRAMESKIP}
									onInput={setField}
									aria-describedby="frameskip_description"
								/>
								<p id="frameskip_description">
									When using the Emulator Data Protocol, let the emulator wait X frames between
									refreshing the memory data for Poke-A-Byte to read.Increasing this value can reduce
									load on the emulator. <br />
									"-1" means a platform specific default frameskip is used (recommended).
									"0" disables frameskip.
								</p>
							</td>
						</tr>

					</tbody>
				</table>
				<button class="wide-button green margin-right" type="submit">
					Save settings
				</button>
				<WideButton onClick={() => setDialog(true)} text="Reset settings" color="red" />
				<ConfirmationModal
					display={dialog}
					title="Warning"
					confirmLabel="Reset"
					text={
						<p>
							Are you sure you want to delete your current settings and set them to default?
						</p>
					}
					onCancel={() => setDialog(false)}
					onConfirm={doReset.call}
				/>
			</form>
		</Panel>
	)
}