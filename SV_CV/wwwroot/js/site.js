(() => {
    const form = document.querySelector('#cv-form');
    const preview = document.querySelector('#cv-preview');
    if (!form || !preview) return;

    const fields = ['FullName', 'Email', 'Phone', 'Location', 'JobTitle', 'Summary', 'Education', 'Skills', 'Projects', 'Template'];
    const valueOf = name => form.elements[name]?.value.trim() || '';

    function render() {
        fields.forEach(name => {
            const element = preview.querySelector(`[data-preview="${name}"]`);
            if (element && valueOf(name)) element.textContent = valueOf(name);
        });

        ['Education', 'Skills', 'Projects'].forEach(name => {
            const container = preview.querySelector(`[data-preview-lines="${name}"]`);
            const section = preview.querySelector(`[data-section="${name}"]`);
            if (!container) return;
            container.replaceChildren();
            const lines = valueOf(name).split('\n').map(line => line.trim()).filter(Boolean);
            lines.forEach(line => {
                const item = document.createElement('div');
                item.textContent = line;
                container.appendChild(item);
            });
            section?.classList.toggle('is-empty', lines.length === 0);
        });
    }

    function selectTemplate(template) {
        const selected = template === 'modern' ? 'modern' : 'classic';
        form.elements.Template.value = selected;
        preview.classList.remove('template-classic', 'template-modern');
        preview.classList.add(`template-${selected}`);
        document.querySelectorAll('.template-option').forEach(button => button.classList.toggle('active', button.dataset.template === selected));
    }

    async function saveDraft() {
        const status = document.querySelector('#saved-status');
        const data = new FormData(form);
        const response = await fetch('/Home/SaveDraft', { method: 'POST', body: data });
        if (!response.ok) throw new Error('save-failed');
        status.textContent = 'Đã lưu lúc ' + new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    }

    function fillForm(documentData) {
        fields.forEach(name => {
            const element = form.elements[name];
            const key = name.charAt(0).toLowerCase() + name.slice(1);
            if (element && documentData[key] !== undefined) element.value = documentData[key] ?? '';
        });
        selectTemplate(valueOf('Template'));
        render();
    }

    form.addEventListener('input', render);
    document.querySelectorAll('.template-option').forEach(button => button.addEventListener('click', () => {
        selectTemplate(button.dataset.template);
        render();
    }));
    document.querySelector('#print-button')?.addEventListener('click', () => window.print());
    document.querySelector('#save-button')?.addEventListener('click', async event => {
        const button = event.currentTarget;
        button.disabled = true;
        try {
            await saveDraft();
        } catch {
            document.querySelector('#saved-status').textContent = 'Không thể lưu';
        } finally {
            button.disabled = false;
        }
    });
    document.querySelector('#load-button')?.addEventListener('click', async event => {
        const button = event.currentTarget;
        button.disabled = true;
        try {
            const response = await fetch('/Home/LoadDraft');
            if (!response.ok) throw new Error('not-found');
            fillForm(await response.json());
            document.querySelector('#saved-status').textContent = 'Đã nạp bản nháp';
        } catch {
            document.querySelector('#saved-status').textContent = 'Chưa có bản nháp';
        } finally {
            button.disabled = false;
        }
    });

    selectTemplate(valueOf('Template'));
    render();
})();