import { PersonRequestBag } from "./types.partial";
import { CommunicationPreference } from "@Obsidian/Enums/Blocks/Crm/FamilyPreRegistration/communicationPreference";
import { Gender } from "@Obsidian/Enums/Crm/gender";
import { FamilyPreRegistrationPersonBag } from "@Obsidian/ViewModels/Blocks/Crm/FamilyPreRegistration/familyPreRegistrationPersonBag";
import { FamilyPreRegistrationInitializationBox } from "@Obsidian/ViewModels/Blocks/Crm/FamilyPreRegistration/familyPreRegistrationInitializationBox";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { InjectionKey, Ref, WritableComputedRef, computed } from "vue";

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

export const GetColumnClassInjectionKey: InjectionKey<(columns: number) => string> = Symbol("Family Pre-Registration Get Column Class");

export function useGetColumnClass(config: FamilyPreRegistrationInitializationBox): (columns: number) => string {
    const getColumn = useGetColumns(config);

    return (columns: number): string => {
        return `col-sm-${getColumn(columns)}`;
    };
}

export function useGetColumns(config: FamilyPreRegistrationInitializationBox): (columns: number) => number {
    return (columns: number): number => {
        if ((columns != 3 && columns != 6) || config.columns === 4) {
            return columns;
        }

        if (columns == 6) {
            return columns;
        }

        return columns * 2;
    };
}

export function getNumberAsOrdinalString(numb: number): string {
    const ordinalMaps = {
        1: "first",
        2: "second",
        3: "third",
        4: "fourth",
        5: "fifth",
        6: "sixth",
        7: "seventh",
        8: "eighth",
        9: "ninth",
        10: "tenth",
        11: "eleventh",
        12: "twelfth",
        13: "thirteenth",
        14: "fourteenth",
        15: "fifteenth",
        16: "sixteenth",
        17: "seventeenth",
        18: "eighteenth",
        19: "nineteenth",
        20: "twentieth",
        30: "thirtieth",
        40: "fortieth",
        50: "fiftieth",
        60: "sixtieth",
        70: "seventieth",
        80: "eightieth",
        90: "ninetieth",
        100: "one hundredth",
        1000: "one thousandth",
        1000000: "one millionth",
        1000000000: "one trillionth",
        1000000000000: "one quadrillionth"
    };
    const maps = {
        1: "one",
        2: "two",
        3: "three",
        4: "four",
        5: "five",
        6: "six",
        7: "seven",
        8: "eight",
        9: "nine",
        10: "ten",
        11: "eleven",
        12: "twelve",
        13: "thirteen",
        14: "fourteen",
        15: "fifteen",
        16: "sixteen",
        17: "seventeen",
        18: "eighteen",
        19: "nineteen",
        20: "twenty",
        30: "thirty",
        40: "forty",
        50: "fifty",
        60: "sixty",
        70: "seventy",
        80: "eighty",
        90: "ninety",
        100: "one hundred",
        1000: "one thousand",
        1000000: "one million",
        1000000000: "one billion",
        1000000000000: "one trillion",
        1000000000000000: "one quadrillion"
    };

    const oneHundred = 100;
    const oneThousand = 1000;
    const oneMillion = 1000000;
    const oneBillion = 1000000000;
    const oneTrillion = 1000000000000;
    const oneQuadrillion = 1000000000000000;

    if (ordinalMaps[numb]) {
        return ordinalMaps[numb];
    }

    function getQuadrillionth(numb: number): string {
        const trillionth = getTrillionth(numb);
        if (numb >= oneQuadrillion) {
            const quadrillions = getHundredsString(Number(numb.toString().slice(-18, -15)));
            if (trillionth) {
                return `${quadrillions} quadrillion ${trillionth}`;
            }
            else {
                return `${quadrillions} quadrillionth`;
            }
        }
        return trillionth;
    }

    function getTrillionth(numb: number): string {
        numb = Number(numb.toString().slice(-15));
        const billionth = getBillionth(numb);
        if (numb >= oneTrillion) {
            const trillions = getHundredsString(Number(numb.toString().slice(-15, -12)));
            if (billionth) {
                return `${trillions} trillion ${billionth}`;
            }
            else {
                return `${trillions} trillionth`;
            }
        }
        return billionth;
    }

    function getBillionth(numb: number): string {
        numb = Number(numb.toString().slice(-12));
        const millionth = getMillionth(numb);
        if (numb >= oneBillion) {
            const billions = getHundredsString(Number(numb.toString().slice(-12, -9)));
            if (millionth) {
                return `${billions} billion ${millionth}`;
            }
            else {
                return `${billions} billionth`;
            }
        }
        return millionth;
    }

    function getMillionth(numb: number): string {
        numb = Number(numb.toString().slice(-9));
        const thousandths = getThousandths(numb);
        if (numb >= oneMillion) {
            const millions = getHundredsString(Number(numb.toString().slice(-9, -6)));
            if (thousandths) {
                return `${millions} million ${thousandths}`;
            }
            else {
                return `${millions} millionth`;
            }
        }
        return thousandths;
    }

    function getThousandths(numb: number): string {
        numb = Number(numb.toString().slice(-6));
        const hundredths = getHundredths(numb);
        if (numb >= oneThousand) {
            const thousands = getHundredsString(Number(numb.toString().slice(-6, -3)));
            if (hundredths) {
                return `${thousands} thousand ${hundredths}`;
            }
            else {
                return `${thousands} thousandths`;
            }
        }
        return hundredths;
    }

    function getHundredths(numb: number): string {
        numb = Number(numb.toString().slice(-3));

        if (ordinalMaps[numb]) {
            return ordinalMaps[numb];
        }

        const tenths = getTenths(numb);
        if (numb >= oneHundred) {
            const hundreds = Number(numb.toString().slice(-3, -2));
            if (tenths) {
                return `${maps[hundreds]} hundred ${tenths}`;
            }
            else {
                return `${maps[hundreds]} hundredth`;
            }
        }
        return tenths;
    }

    function getHundredsString(numb: number): string {
        numb = Number(numb.toString().slice(-3));

        if (maps[numb]) {
            return maps[numb];
        }

        const tens = getTensString(numb);

        if (numb >= oneHundred) {
            const hundreds = Number(numb.toString().slice(-3, -2));
            if (tens) {
                return `${maps[hundreds]} hundred ${tens}`;
            }
            else {
                return `${maps[hundreds]} hundred`;
            }
        }
        return tens;
    }

    function getTensString(numb: number): string {
        numb = Number(numb.toString().slice(-2));

        if (maps[numb]) {
            return maps[numb];
        }

        const ones = getOnesString(numb);

        if (numb >= 20) {
            const tens = Number(numb.toString().slice(-2, -1));

            if (ones) {
                return `${maps[tens * 10]}-${ones}`;
            }
            else {
                return maps[tens * 10];
            }
        }
        return ones;
    }

    function getTenths(numb: number): string {
        numb = Number(numb.toString().slice(-2));

        if (ordinalMaps[numb]) {
            return ordinalMaps[numb];
        }

        const oneths = getOneths(numb);

        if (numb >= 20) {
            const tens = Number(numb.toString().slice(-2, -1));

            if (oneths) {
                return `${maps[tens * 10]}-${oneths}`;
            }
            else {
                return ordinalMaps[tens * 10];
            }
        }
        return oneths;
    }

    function getOneths(numb: number): string {
        numb = Number(numb.toString().slice(-1));

        return ordinalMaps[numb];
    }

    function getOnesString(numb: number): string {
        numb = Number(numb.toString().slice(-1));

        return maps[numb];
    }

    return getQuadrillionth(numb);
}