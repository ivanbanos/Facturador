const GetToken = async (username, password) => {
  try {
    // Las credenciales van en el cuerpo: no quedan en la URL y admiten cualquier caracter
    const response = await fetch(`${window.SERVER_URL}/Usuarios/Login`, {
      method: 'POST',
      mode: 'cors',
      headers: { accept: 'application/json', 'Content-Type': 'application/json' },
      body: JSON.stringify({ usuario: username, contrasena: password }),
    })
    if (response.status === 200) {
      return await response.json()
    }
    return null
  } catch (error) {
    console.error('Error al iniciar sesión:', error)
    return null
  }
}

export default GetToken
