import { Layer, PM } from "leaflet";
import { useEffect } from "react";

export default function EnableOrDisableLayers({layers, enabled, whenEnabledOptions, whenDisabledOptions}: {
    layers: Layer[];
    enabled: boolean;
    whenEnabledOptions: PM.EditModeOptions;
    whenDisabledOptions: PM.EditModeOptions
}) {
    useEffect(() => {
        layers.forEach(layer => {
            if (enabled) {
                layer.options.pmIgnore = false;
                // @ts-ignore
                if (layer.options.interactive !== undefined) {
                    // @ts-ignore
                    layer.options.interactive = true;
                }
                // @ts-ignore
                if (layer.pm) {
                    // @ts-ignore
                    layer.pm.setOptions(whenEnabledOptions);
                }
            } else {
                layer.options.pmIgnore = true;

                // @ts-ignore
                if (layer.options.interactive !== undefined) {
                    // @ts-ignore
                    layer.options.interactive = false;
                }
                // @ts-ignore
                if (layer.pm) {
                    // @ts-ignore
                    layer.pm.setOptions(whenEnabledOptions);
                    // @ts-ignore
                    layer.pm.disable();
                }
            }
        });
    }, [layers, enabled, whenEnabledOptions, whenDisabledOptions]);

    return <></>
}