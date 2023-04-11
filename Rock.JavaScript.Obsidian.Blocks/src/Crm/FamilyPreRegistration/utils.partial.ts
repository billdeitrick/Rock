import { PersonRequestBag } from "./types.partial";
import { CommunicationPreference } from "@Obsidian/Enums/Blocks/Crm/FamilyPreRegistration/communicationPreference";
import { Gender } from "@Obsidian/Enums/Crm/gender";
import { FamilyPreRegistrationPersonBag } from "@Obsidian/ViewModels/Blocks/Crm/FamilyPreRegistration/familyPreRegistrationPersonBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { Ref, WritableComputedRef, computed } from "vue";

export function convertPersonToPersonRequest(person: FamilyPreRegistrationPersonBag | null | undefined): PersonRequestBag {
    const defaults = createPersonRequest();

    return {
        // Copy values from person bag.
        ...person,

        // Overwrite required fields.
        attributeValues: person?.attributeValues || defaults.attributeValues,
        communicationPreference: person?.communicationPreference ?? defaults.communicationPreference,
        email: person?.email || defaults.email,
        firstName: person?.firstName || defaults.firstName,
        gender: person?.gender || defaults.gender,
        isFirstNameReadOnly: person?.isFirstNameReadOnly || defaults.isFirstNameReadOnly,
        isLastNameReadOnly: person?.isLastNameReadOnly || defaults.isLastNameReadOnly,
        lastName: person?.lastName || defaults.lastName,
        mobilePhone: person?.mobilePhone || defaults.mobilePhone,
        mobilePhoneCountryCode: person?.mobilePhoneCountryCode || defaults.mobilePhoneCountryCode,
    };
}

export function createPersonRequest(): PersonRequestBag {
    return {
        attributeValues: {},
        communicationPreference: CommunicationPreference.None,
        email: "",
        firstName: "",
        gender: Gender.Unknown,
        isFirstNameReadOnly: false,
        isLastNameReadOnly: false,
        lastName: "",
        mobilePhone: "",
        mobilePhoneCountryCode: ""
    };
}

export function createListItemBagWrapper<
    TObj extends Record<string, unknown>,
    TKey extends keyof TObj>(r: Ref<TObj>, p: TKey): WritableComputedRef<ListItemBag> {
    return computed<ListItemBag>({
        get() {
            const value = r.value;

            return {
                value: value ? value[p]?.toString() : undefined
            };
        },
        set(newValue: ListItemBag) {
            try {
                const value = r.value;
                if (value) {
                    Object.assign(value, { [p]: newValue.value });
                }
            }
            catch (e) {
                console.error("TODO JMH Should we show error here?", e);
            }
        }
    });
}