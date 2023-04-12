import { standardColumnProps } from "@Obsidian/Core/Controls/grid";
import { defineComponent, PropType, VNode } from "vue";
import ReorderCell from "../Cells/reorderCell.partial.obs";

export default defineComponent({
    props: {
        ...standardColumnProps,

        name: {
            type: String as PropType<string>,
            default: "__reorder"
        },

        format: {
            type: Object as PropType<VNode>,
            default: ReorderCell
        },

        headerClass: {
            type: String as PropType<string>,
            default: "grid-columnreorder"
        },

        itemClass: {
            type: String as PropType<string>,
            default: "grid-columnreorder"
        },

        onOrderChanged: {
            type: Function as PropType<(item: Record<string, unknown>, beforeItem: Record<string, unknown> | null, order: number) => void | Promise<void> | boolean | Promise<boolean>>,
            required: false
        }
    }
});
