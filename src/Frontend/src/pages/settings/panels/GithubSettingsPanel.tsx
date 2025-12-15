
import type { JSX, TargetedInputEvent } from "preact";
import { useCallback, useEffect, useReducer, useRef, useState } from "preact/hooks";
import { Store } from "@/utility/propertyStore";
import { useAPI } from "@/hooks/useAPI";
import { GithubSettings } from "pokeaclient";
import { ConfirmationModal } from "@/components/ConfirmationModal";
import { WideButton } from "@/components/WideButton";
import { Panel } from "@/components/Panel";

export function GithubSettingsPanel() {
	const filesClient = Store.client.files;
	const [status, setStatus] = useState({ color: "", message: "" });
	const [formState, setFieldState] = useReducer(
		(data, update: { fieldId: keyof GithubSettings, value: string }) => {
			const newState = structuredClone(data);
			newState[update.fieldId] = update.value;
			return newState;
		},
		{
			owner: "", repo: "", dir: "", token: "", accept: "", api_version: "",
		},
	);

	const setField = (e: TargetedInputEvent<HTMLInputElement>) => {
		if (e.currentTarget) {
			setFieldState({ fieldId: e.currentTarget.name as keyof GithubSettings, value: e.currentTarget.value })
		}
	};
	const githubSettings = useAPI(Store.client.files.getGithubSettings);
	const githubSettingsResult = githubSettings.result;
	useEffect(() => {
		setFieldState({ fieldId: "owner", value: githubSettingsResult?.owner ?? "" });
		setFieldState({ fieldId: "repo", value: githubSettingsResult?.repo ?? "" });
		setFieldState({ fieldId: "dir", value: githubSettingsResult?.dir ?? "" });
		setFieldState({ fieldId: "token", value: githubSettingsResult?.token ?? "" });
		setFieldState({ fieldId: "accept", value: githubSettingsResult?.accept ?? "" });
		setFieldState({ fieldId: "api_version", value: githubSettingsResult?.api_version ?? "" });
	}, [githubSettingsResult, setFieldState])
	const [clearModal, setClearModal] = useState<boolean>(false);
	const formRef = useRef<HTMLFormElement | null>(null);
	const formData = formRef.current ? new FormData(formRef.current) : null;
	useEffect(
		() => githubSettings.call(),
		// eslint-disable-next-line react-hooks/exhaustive-deps
		[]
	);
	const clearSettings = useCallback(() => {
		filesClient.saveGithubSettings(formState).then(
			(ok) => {
				if (ok) {
					setStatus({ color: "success-text", message: "Successfully cleared settings!" });
				} else {
					setStatus({ color: "error-text", message: "Failed to save github settings!!" });
				}
			}
		);
		setClearModal(false);
		githubSettings.call();
	}, [githubSettings, filesClient, formState]);

	if (!githubSettings.wasCalled || githubSettings.isLoading) {
		return <></>;
	}

	const openGithubLink = () => {
		filesClient.getGithubLink().then(uri => {
			if (uri !== null) {
				window.open(uri, "_blank")
			}
		});
	}
	const testGithubSettings = () => {
		filesClient.testGithubSettingsAsync().then(response => {
			if (response?.startsWith("Successfully")) {
				setStatus({ color: "success-text", message: response });
			} else if (response?.startsWith("Failed")) {
				setStatus({ color: "error-text", message: response });
			}
		});
	}
	const onSubmit = (event: JSX.TargetedSubmitEvent<HTMLFormElement>) => {
		filesClient.saveGithubSettings({
			owner: formData?.get("owner") as string ?? "",
			repo: formData?.get("repo") as string ?? "",
			dir: formData?.get("dir") as string ?? "",
			token: formData?.get("token") as string ?? "",
			accept: githubSettings.result?.accept || "",
			api_version: githubSettings.result?.api_version || "",
		}).then(
			(ok) => {
				if (ok) {
					setStatus({ color: "success-text", message: "Successfully saved changes!" });
				} else {
					setStatus({ color: "error-text", message: "Failed to save github settings!!" });
				}
			}
		)
		event.preventDefault();
		return false;
	}
	return (
		<Panel id="settings_gh" title="GitHub API settings" defaultOpen>
			<div>
				<span class="text-red">
					Changing these settings is generally not recommended unless you know what you are doing.
					<br />
					These settings allow you to set a custom mapper repository, incorrect settings can cause issues retrieving
					new and updated mappers. <strong>Please only do so with caution.</strong>
				</span>
				<hr />
				<form onSubmit={onSubmit} ref={formRef}>
					<table>
						<tbody>
							<tr>
								<th>
									<label htmlFor="owner">GitHub Account Name: </label>
								</th>
								<td>
									<input name="owner" type="text" value={formState.owner} onInput={setField} />
								</td>
							</tr>
							<tr>
								<th>
									<label htmlFor="repo">Repository Name:</label>
								</th>
								<td>
									<input name="repo" type="text" value={formState.repo} onInput={setField} />
								</td>
							</tr>
							<tr>
								<th>
									<label htmlFor="dir">Alternative Directory Name:</label>
								</th>
								<td>
									<input name="dir" type="text" value={formState.dir} onInput={setField} />
									<br />
									<small>
										Note: This is only required if the mapper_tree.json is not in the root directory of the repository.
									</small>
								</td>
							</tr>
							<tr>
								<th>
									<label htmlFor="token">Personal Access Token:</label>
								</th>
								<td>
									<input name="token" type="text" value={formState.token} onInput={setField} />
									<br />
									<small>
										Note: This is required if the repository is private or if you want to bypass the
										GitHub REST Api rate limit. Once you have a token generated do not share it with anyone; for more
										information please refer to this page:{" "}
										<a href="https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens">
											Managing your personal access tokens
										</a>
									</small>
								</td>
							</tr>
						</tbody>
					</table>

					<br />
					{status.message !== "" &&
						<span class={status.color}>{status.message}</span>
					}
					<br />
					<WideButton text="Test settings" color="blue" onClick={testGithubSettings} />
					<WideButton text="Open GitHub link" color="purple" onClick={openGithubLink} />
					<WideButton text="Save settings" color="green" onClick={openGithubLink} />
					<WideButton text="Clear settings" color="red" onClick={() => setClearModal(true)} />
					<ConfirmationModal
						display={clearModal}
						title="Warning"
						confirmLabel="Reset"
						text={
							<p>
								Are you sure you want to delete your current settings and set them to default?
							</p>
						}
						onCancel={() => setClearModal(false)}
						onConfirm={() => clearSettings()}
					/>
				</form>
			</div>
		</Panel>
	)
}