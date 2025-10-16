function getPatients() {
    fetch('/api/patient')
        .then(res => res.json())
        .then(data => alert(JSON.stringify(data)))
        .catch(err => console.error(err));
}
