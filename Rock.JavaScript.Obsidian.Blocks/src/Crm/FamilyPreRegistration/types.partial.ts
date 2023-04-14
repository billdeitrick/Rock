import { FamilyPreRegistrationPersonBag } from "@Obsidian/ViewModels/Blocks/Crm/FamilyPreRegistration/familyPreRegistrationPersonBag";
import { FamilyPreRegistrationCreateAccountRequestBag } from "@Obsidian/ViewModels/Blocks/Crm/FamilyPreRegistration/familyPreRegistrationCreateAccountRequestBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

/**
 * Make specific properties in T required and non-nullable.
 *
 * @example
 * type Shape = {
 *  length?: number | null,
 *  width?: number | null
 * };
 *
 * type StaticShape = RequiredProperties<Shape, "length" | "width">;
 * // {
 * //   length: number,
 * //   width: number
 * // }
 */
export type RequiredProperties<T, K extends keyof T> = Omit<T, K> & {
    [L in keyof Pick<T, K>]-?: NonNullable<T[L]>
};

export type PersonRequestBag = RequiredProperties<
    FamilyPreRegistrationPersonBag,
    "firstName"
    | "lastName"
    | "email"
    | "mobilePhone"
    | "mobilePhoneCountryCode"
    | "attributeValues"
>;

export type ChildRequestBag = RequiredProperties<PersonRequestBag, "familyRoleGuid">;

export type CreateAccountRequest = RequiredProperties<
    FamilyPreRegistrationCreateAccountRequestBag,
    "username"
    | "password"
>;
