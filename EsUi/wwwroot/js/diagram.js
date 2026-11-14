window.centerMermaidDiagram = (container) => {
    if (!container) {
        return;
    }

    const svg = container.querySelector('svg');

    if (!svg) {
        return;
    }

    const svgWidth = svg.getBoundingClientRect().width;

    // Корень дерева находится в середине широкой диаграммы, поэтому
    // показываем её середину, а не пустой левый край.
    container.scrollLeft = Math.max(0, (svgWidth - container.clientWidth) / 2);
};
